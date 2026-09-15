using Moq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TimeLog.Domain;
using Nop.Plugin.Misc.TimeLog.Services;
using Nop.Plugin.Misc.TimeLog.Validators;
using Nop.Services.Localization;
using Xunit;
using TimeLogEntity = Nop.Plugin.Misc.TimeLog.Domain.TimeLog;

namespace Nop.Plugin.Misc.TimeLog.Tests.Services;

/// <summary>
/// Unit tests for <see cref="TimeLogService"/> - covers the ownership filter (TT-006),
/// the Draft-only guard on update/delete, and the <see cref="TimeLogService.SubmitTimeLogsAsync"/>
/// partial success/failure aggregation (TT-008).
/// </summary>
public class TimeLogServiceTests
{
    private const int OwnerCustomerId = 10;
    private const int OtherCustomerId = 20;
    private const int ActiveProjectId = 1;

    private static Mock<IRepository<TimeLogEntity>> CreateRepositoryMock(List<TimeLogEntity> data)
    {
        var repositoryMock = new Mock<IRepository<TimeLogEntity>>();

        //Table must reflect live list contents so UpdateAsync-driven mutations are observable by later queries
        repositoryMock.Setup(r => r.Table).Returns(() => data.AsQueryable());

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int?>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync((int? id, Func<ICacheKeyService, CacheKey> _, bool _, bool _) =>
                data.FirstOrDefault(t => t.Id == id));

        repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TimeLogEntity>(), It.IsAny<bool>()))
            .Callback((TimeLogEntity entity, bool _) =>
            {
                var index = data.FindIndex(t => t.Id == entity.Id);
                if (index >= 0)
                    data[index] = entity;
            })
            .Returns(Task.CompletedTask);

        repositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<TimeLogEntity>(), It.IsAny<bool>()))
            .Callback((TimeLogEntity entity, bool _) => data.RemoveAll(t => t.Id == entity.Id))
            .Returns(Task.CompletedTask);

        repositoryMock
            .Setup(r => r.InsertAsync(It.IsAny<TimeLogEntity>(), It.IsAny<bool>()))
            .Callback((TimeLogEntity entity, bool _) =>
            {
                entity.Id = data.Count == 0 ? 1 : data.Max(t => t.Id) + 1;
                data.Add(entity);
            })
            .Returns(Task.CompletedTask);

        return repositoryMock;
    }

    private static TimeLogValidator CreateValidator(bool projectActive = true)
    {
        var localizationServiceMock = new Mock<ILocalizationService>();
        localizationServiceMock
            .Setup(x => x.GetResourceAsync(It.IsAny<string>()))
            .ReturnsAsync((string key) => key);

        var projectServiceMock = new Mock<IProjectService>();
        projectServiceMock
            .Setup(x => x.GetProjectByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Project { Id = ActiveProjectId, Name = "Project", Active = projectActive });

        return new TimeLogValidator(localizationServiceMock.Object, projectServiceMock.Object);
    }

    private static TimeLogEntity BuildTimeLog(int id, int customerId, TimeLogStatus status,
        int projectId = ActiveProjectId, string task = "Development", decimal time = 8m)
    {
        return new TimeLogEntity
        {
            Id = id,
            CustomerId = customerId,
            ProjectId = projectId,
            Task = task,
            Date = DateTime.UtcNow.Date,
            Time = time,
            Status = status,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };
    }

    private static (TimeLogService service, List<TimeLogEntity> data, Mock<IProjectService> projectServiceMock)
        CreateService(List<TimeLogEntity> seedData, bool projectActive = true)
    {
        return CreateServiceWithProject(seedData,
            new Project { Id = ActiveProjectId, Name = "Project", Active = projectActive, Status = ProjectStatus.NotStarted });
    }

    /// <summary>
    /// BUG-008: variant of <see cref="CreateService"/> that lets a test control the mocked project's
    /// <see cref="Project.Status"/> directly, independently of the legacy <see cref="Project.Active"/>
    /// flag, so the Submit-path eligibility check (which now reads Status via
    /// <see cref="NopTimeLogDefaults.TimeLoggableProjectStatuses"/>) can be exercised without also
    /// tripping the separate, out-of-scope Active-based check in <see cref="TimeLogValidator"/>.
    /// </summary>
    private static (TimeLogService service, List<TimeLogEntity> data, Mock<IProjectService> projectServiceMock)
        CreateServiceWithProject(List<TimeLogEntity> seedData, Project project)
    {
        var localizationServiceMock = new Mock<ILocalizationService>();
        localizationServiceMock
            .Setup(x => x.GetResourceAsync(It.IsAny<string>()))
            .ReturnsAsync((string key) => key);

        var projectServiceMock = new Mock<IProjectService>();
        projectServiceMock
            .Setup(x => x.GetProjectByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(project);

        var repositoryMock = CreateRepositoryMock(seedData);
        var validator = new TimeLogValidator(localizationServiceMock.Object, projectServiceMock.Object);

        var service = new TimeLogService(localizationServiceMock.Object, projectServiceMock.Object,
            repositoryMock.Object, validator);

        return (service, seedData, projectServiceMock);
    }

    #region GetOwnTimeLogByIdAsync - ownership filter

    [Fact]
    public async Task GetOwnTimeLogByIdAsync_OwnedRecord_ReturnsRecord()
    {
        var data = new List<TimeLogEntity>
        {
            BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft),
            BuildTimeLog(2, OtherCustomerId, TimeLogStatus.Draft)
        };
        var (service, _, _) = CreateService(data);

        var result = await service.GetOwnTimeLogByIdAsync(1, OwnerCustomerId);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetOwnTimeLogByIdAsync_RecordOwnedByAnotherCustomer_ReturnsNull()
    {
        //two rows exist - one owned by the requester, one not - only the owned one may ever be returned
        var data = new List<TimeLogEntity>
        {
            BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft),
            BuildTimeLog(2, OtherCustomerId, TimeLogStatus.Draft)
        };
        var (service, _, _) = CreateService(data);

        var result = await service.GetOwnTimeLogByIdAsync(2, OwnerCustomerId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOwnTimeLogByIdAsync_NonexistentId_ReturnsNull()
    {
        var data = new List<TimeLogEntity> { BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft) };
        var (service, _, _) = CreateService(data);

        var result = await service.GetOwnTimeLogByIdAsync(999, OwnerCustomerId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOwnTimeLogByIdAsync_IdLessThanOrEqualToZero_ReturnsNullWithoutQuerying()
    {
        var data = new List<TimeLogEntity> { BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft) };
        var (service, _, _) = CreateService(data);

        var result = await service.GetOwnTimeLogByIdAsync(0, OwnerCustomerId);

        Assert.Null(result);
    }

    #endregion

    #region UpdateTimeLogAsync / DeleteTimeLogAsync - Draft-only guard

    [Fact]
    public async Task UpdateTimeLogAsync_DraftRecord_Succeeds()
    {
        var data = new List<TimeLogEntity> { BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft) };
        var (service, _, _) = CreateService(data);
        var timeLog = data[0];
        timeLog.Task = "Updated task";

        await service.UpdateTimeLogAsync(timeLog);

        Assert.Equal("Updated task", data[0].Task);
    }

    [Fact]
    public async Task UpdateTimeLogAsync_SubmittedRecord_ThrowsInvalidOperationException()
    {
        var data = new List<TimeLogEntity> { BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Submitted) };
        var (service, _, _) = CreateService(data);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateTimeLogAsync(data[0]));
    }

    [Fact]
    public async Task DeleteTimeLogAsync_DraftRecord_Succeeds()
    {
        var data = new List<TimeLogEntity> { BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft) };
        var (service, _, _) = CreateService(data);

        await service.DeleteTimeLogAsync(data[0]);

        Assert.Empty(data);
    }

    [Fact]
    public async Task DeleteTimeLogAsync_SubmittedRecord_ThrowsInvalidOperationException()
    {
        var data = new List<TimeLogEntity> { BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Submitted) };
        var (service, _, _) = CreateService(data);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteTimeLogAsync(data[0]));

        Assert.Single(data);
    }

    #endregion

    #region InsertTimeLogAsync / UpdateTimeLogAsync - field validation (BUG-001/AC-10)

    [Fact]
    public async Task InsertTimeLogAsync_EmptyTask_ThrowsInvalidOperationExceptionAndDoesNotPersist()
    {
        var data = new List<TimeLogEntity>();
        var (service, _, _) = CreateService(data);
        var timeLog = BuildTimeLog(0, OwnerCustomerId, TimeLogStatus.Draft, task: string.Empty);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.InsertTimeLogAsync(timeLog));

        Assert.Empty(data);
    }

    [Fact]
    public async Task InsertTimeLogAsync_TimeOutOfRange_ThrowsInvalidOperationExceptionAndDoesNotPersist()
    {
        var data = new List<TimeLogEntity>();
        var (service, _, _) = CreateService(data);
        var timeLog = BuildTimeLog(0, OwnerCustomerId, TimeLogStatus.Draft, time: 25m);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.InsertTimeLogAsync(timeLog));

        Assert.Empty(data);
    }

    [Fact]
    public async Task InsertTimeLogAsync_InactiveProject_ThrowsInvalidOperationExceptionAndDoesNotPersist()
    {
        var data = new List<TimeLogEntity>();
        var (service, _, _) = CreateService(data, projectActive: false);
        var timeLog = BuildTimeLog(0, OwnerCustomerId, TimeLogStatus.Draft);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.InsertTimeLogAsync(timeLog));

        Assert.Empty(data);
    }

    [Fact]
    public async Task InsertTimeLogAsync_ValidEntity_PersistsSuccessfully()
    {
        var data = new List<TimeLogEntity>();
        var (service, _, _) = CreateService(data);
        var timeLog = BuildTimeLog(0, OwnerCustomerId, TimeLogStatus.Draft);

        await service.InsertTimeLogAsync(timeLog);

        Assert.Single(data);
    }

    [Fact]
    public async Task UpdateTimeLogAsync_EmptyTask_ThrowsInvalidOperationExceptionAndLeavesRecordUnchanged()
    {
        var data = new List<TimeLogEntity> { BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft) };
        var (service, _, _) = CreateService(data);

        var updated = BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft, task: string.Empty);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateTimeLogAsync(updated));

        //the persisted record must remain untouched - no partial persistence of the invalid update
        Assert.Equal("Development", data[0].Task);
    }

    [Fact]
    public async Task UpdateTimeLogAsync_TimeOutOfRange_ThrowsInvalidOperationExceptionAndLeavesRecordUnchanged()
    {
        var data = new List<TimeLogEntity> { BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft) };
        var (service, _, _) = CreateService(data);

        var updated = BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft, time: -1m);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateTimeLogAsync(updated));

        Assert.Equal(8m, data[0].Time);
    }

    [Fact]
    public async Task UpdateTimeLogAsync_InactiveOrInvalidProject_ThrowsInvalidOperationExceptionAndLeavesRecordUnchanged()
    {
        var data = new List<TimeLogEntity> { BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft) };
        var (service, _, _) = CreateService(data, projectActive: false);

        var updated = BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateTimeLogAsync(updated));

        Assert.Equal(ActiveProjectId, data[0].ProjectId);
    }

    #endregion

    #region SubmitTimeLogsAsync - partial success/failure aggregation

    [Fact]
    public async Task SubmitTimeLogsAsync_MixedValidInvalidAndNotOwnedIds_ReturnsCorrectPerIdResultsWithoutThrowing()
    {
        var data = new List<TimeLogEntity>
        {
            //id 1: valid draft, owned - should succeed
            BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft),
            //id 2: already submitted, owned - should fail (not draft)
            BuildTimeLog(2, OwnerCustomerId, TimeLogStatus.Submitted),
            //id 3: draft, owned, but fails field validation (empty task)
            BuildTimeLog(3, OwnerCustomerId, TimeLogStatus.Draft, task: string.Empty),
            //id 4: draft, but owned by a DIFFERENT customer - must be treated as not found
            BuildTimeLog(4, OtherCustomerId, TimeLogStatus.Draft)
        };
        var (service, _, _) = CreateService(data);

        var idsToSubmit = new List<int> { 1, 2, 3, 4, 999 }; //999 = nonexistent id entirely

        var results = await service.SubmitTimeLogsAsync(idsToSubmit, OwnerCustomerId);

        Assert.Equal(5, results.Count);

        Assert.True(results.Single(r => r.TimeLogId == 1).Success);
        Assert.False(results.Single(r => r.TimeLogId == 2).Success);
        Assert.False(results.Single(r => r.TimeLogId == 3).Success);
        Assert.False(results.Single(r => r.TimeLogId == 4).Success);
        Assert.False(results.Single(r => r.TimeLogId == 999).Success);

        //the one successful row must actually have been transitioned to Submitted
        Assert.Equal(TimeLogStatus.Submitted, data.Single(t => t.Id == 1).Status);

        //rows that failed must be left untouched
        Assert.Equal(TimeLogStatus.Submitted, data.Single(t => t.Id == 2).Status);
        Assert.Equal(TimeLogStatus.Draft, data.Single(t => t.Id == 3).Status);
        Assert.Equal(TimeLogStatus.Draft, data.Single(t => t.Id == 4).Status);
    }

    [Fact]
    public async Task SubmitTimeLogsAsync_ProjectNoLongerActive_FailsWithProjectInactiveResult()
    {
        var data = new List<TimeLogEntity> { BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft) };
        var (service, _, _) = CreateService(data, projectActive: false);

        var results = await service.SubmitTimeLogsAsync(new List<int> { 1 }, OwnerCustomerId);

        Assert.False(results.Single().Success);
        Assert.Equal(TimeLogStatus.Draft, data[0].Status);
    }

    [Theory]
    [InlineData(ProjectStatus.OnHold)]
    [InlineData(ProjectStatus.Completed)]
    [InlineData(ProjectStatus.Cancelled)]
    [InlineData(ProjectStatus.Retired)]
    public async Task SubmitTimeLogsAsync_ProjectStatusNoLongerEligible_FailsEvenWhenProjectStillActive(
        ProjectStatus ineligibleStatus)
    {
        //BUG-008 regression guard: Project.Active is left true (as ProjectController sets it
        //unconditionally on every save) but Status has moved past NotStarted/Inprogress - Submit must
        //now be rejected based on Status, not the stale Active flag
        var data = new List<TimeLogEntity> { BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft) };
        var project = new Project { Id = ActiveProjectId, Name = "Project", Active = true, Status = ineligibleStatus };
        var (service, _, _) = CreateServiceWithProject(data, project);

        var results = await service.SubmitTimeLogsAsync(new List<int> { 1 }, OwnerCustomerId);

        Assert.False(results.Single().Success);
        Assert.Equal(TimeLogStatus.Draft, data[0].Status);
    }

    [Theory]
    [InlineData(ProjectStatus.NotStarted)]
    [InlineData(ProjectStatus.Inprogress)]
    public async Task SubmitTimeLogsAsync_ProjectStatusStillEligible_Succeeds(ProjectStatus eligibleStatus)
    {
        //regression guard: legitimate submits against a NotStarted/Inprogress project must keep working
        var data = new List<TimeLogEntity> { BuildTimeLog(1, OwnerCustomerId, TimeLogStatus.Draft) };
        var project = new Project { Id = ActiveProjectId, Name = "Project", Active = true, Status = eligibleStatus };
        var (service, _, _) = CreateServiceWithProject(data, project);

        var results = await service.SubmitTimeLogsAsync(new List<int> { 1 }, OwnerCustomerId);

        Assert.True(results.Single().Success);
        Assert.Equal(TimeLogStatus.Submitted, data[0].Status);
    }

    [Fact]
    public async Task SubmitTimeLogsAsync_EmptyIdList_ReturnsEmptyResultsWithoutThrowing()
    {
        var data = new List<TimeLogEntity>();
        var (service, _, _) = CreateService(data);

        var results = await service.SubmitTimeLogsAsync(new List<int>(), OwnerCustomerId);

        Assert.Empty(results);
    }

    [Fact]
    public async Task SubmitTimeLogsAsync_NullIdList_ReturnsEmptyResultsWithoutThrowing()
    {
        var data = new List<TimeLogEntity>();
        var (service, _, _) = CreateService(data);

        var results = await service.SubmitTimeLogsAsync(null, OwnerCustomerId);

        Assert.Empty(results);
    }

    #endregion
}
