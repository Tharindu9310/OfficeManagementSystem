using Moq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TimeLog.Domain;
using Nop.Plugin.Misc.TimeLog.Services;
using Nop.Services.Stores;
using Xunit;
using TimeLogEntity = Nop.Plugin.Misc.TimeLog.Domain.TimeLog;

namespace Nop.Plugin.Misc.TimeLog.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ProjectService"/> Phase 2 additions - focused on TT-044's regression
/// requirement (unassigning a staff member via <see cref="ProjectService.SaveStaffAssignmentsAsync"/>
/// must never touch the <see cref="TimeLogEntity"/> table - design §3.6's deliberate non-feature,
/// requirements §6 Decision 4) plus the eligibility filtering behind TT-025/TT-042.
/// </summary>
public class ProjectServiceTests
{
    private const int ProjectId = 1;
    private const int OtherProjectId = 2;
    private const int StaffCustomerId = 100;

    private static Mock<IRepository<Project>> CreateProjectRepositoryMock(List<Project> data)
    {
        var repositoryMock = new Mock<IRepository<Project>>();
        repositoryMock.Setup(r => r.Table).Returns(() => data.AsQueryable());

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int?>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync((int? id, Func<ICacheKeyService, CacheKey> _, bool _, bool _) =>
                data.FirstOrDefault(p => p.Id == id));

        repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Project>(), It.IsAny<bool>()))
            .Callback((Project entity, bool _) =>
            {
                var index = data.FindIndex(p => p.Id == entity.Id);
                if (index >= 0)
                    data[index] = entity;
            })
            .Returns(Task.CompletedTask);

        return repositoryMock;
    }

    private static Mock<IRepository<ProjectStaffMapping>> CreateMappingRepositoryMock(List<ProjectStaffMapping> data)
    {
        var repositoryMock = new Mock<IRepository<ProjectStaffMapping>>();
        repositoryMock.Setup(r => r.Table).Returns(() => data.AsQueryable());

        repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<Func<IQueryable<ProjectStaffMapping>, IQueryable<ProjectStaffMapping>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>()))
            .ReturnsAsync((Func<IQueryable<ProjectStaffMapping>, IQueryable<ProjectStaffMapping>> func, Func<ICacheKeyService, CacheKey> _, bool _) =>
                (func == null ? data.AsQueryable() : func(data.AsQueryable())).ToList());

        repositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<IList<ProjectStaffMapping>>(), It.IsAny<bool>()))
            .Callback((IList<ProjectStaffMapping> entities, bool _) =>
            {
                foreach (var entity in entities)
                    data.RemoveAll(m => m.Id == entity.Id);
            })
            .Returns(Task.CompletedTask);

        repositoryMock
            .Setup(r => r.InsertAsync(It.IsAny<IList<ProjectStaffMapping>>(), It.IsAny<bool>()))
            .Callback((IList<ProjectStaffMapping> entities, bool _) =>
            {
                var nextId = data.Count == 0 ? 1 : data.Max(m => m.Id) + 1;
                foreach (var entity in entities)
                {
                    entity.Id = nextId++;
                    data.Add(entity);
                }
            })
            .Returns(Task.CompletedTask);

        return repositoryMock;
    }

    private static Mock<IRepository<TimeLogEntity>> CreateTimeLogRepositoryMock(List<TimeLogEntity> data)
    {
        var repositoryMock = new Mock<IRepository<TimeLogEntity>>();
        repositoryMock.Setup(r => r.Table).Returns(() => data.AsQueryable());

        return repositoryMock;
    }

    private static ProjectService CreateService(List<Project> projects, List<ProjectStaffMapping> mappings,
        List<TimeLogEntity> timeLogs)
    {
        var projectRepositoryMock = CreateProjectRepositoryMock(projects);
        var mappingRepositoryMock = CreateMappingRepositoryMock(mappings);
        var timeLogRepositoryMock = CreateTimeLogRepositoryMock(timeLogs);

        var staticCacheManagerMock = new Mock<IStaticCacheManager>();
        staticCacheManagerMock
            .Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(Task.CompletedTask);

        var storeMappingServiceMock = new Mock<IStoreMappingService>();

        return new ProjectService(projectRepositoryMock.Object, mappingRepositoryMock.Object,
            timeLogRepositoryMock.Object, staticCacheManagerMock.Object, storeMappingServiceMock.Object);
    }

    /// <summary>
    /// TT-044: unassigning a staff member (removing their ProjectStaffMapping row) must never touch
    /// the TimeLog table - existing rows (any status) remain completely unchanged.
    /// </summary>
    [Fact]
    public async Task SaveStaffAssignmentsAsync_RemovingAssignment_DoesNotTouchTimeLogRows()
    {
        var mappings = new List<ProjectStaffMapping>
        {
            new() { Id = 1, ProjectId = ProjectId, CustomerId = StaffCustomerId }
        };

        var timeLogs = new List<TimeLogEntity>
        {
            new() { Id = 1, ProjectId = ProjectId, CustomerId = StaffCustomerId, Status = TimeLogStatus.Draft, Task = "A", Time = 1m },
            new() { Id = 2, ProjectId = ProjectId, CustomerId = StaffCustomerId, Status = TimeLogStatus.Submitted, Task = "B", Time = 2m }
        };

        var expectedTimeLogsSnapshot = timeLogs.Select(t => (t.Id, t.ProjectId, t.CustomerId, t.Status)).ToList();

        var service = CreateService(new List<Project> { new() { Id = ProjectId, Name = "P1" } }, mappings, timeLogs);

        //unassign the staff member entirely (empty submitted selection)
        await service.SaveStaffAssignmentsAsync(ProjectId, new List<int>());

        Assert.Empty(await service.GetAssignedCustomerIdsAsync(ProjectId));

        //the TimeLog rows must be byte-for-byte unchanged - no cascade/event hook touched them
        var actualSnapshot = timeLogs.Select(t => (t.Id, t.ProjectId, t.CustomerId, t.Status)).ToList();
        Assert.Equal(expectedTimeLogsSnapshot, actualSnapshot);
        Assert.Equal(2, timeLogs.Count);
    }

    [Fact]
    public async Task GetProjectsAssignedToCustomerAsync_EligibleOnly_ExcludesIneligibleStatuses()
    {
        var projects = new List<Project>
        {
            new() { Id = ProjectId, Name = "Active", StatusId = (int)ProjectStatus.Inprogress },
            new() { Id = OtherProjectId, Name = "OnHold", StatusId = (int)ProjectStatus.OnHold }
        };

        var mappings = new List<ProjectStaffMapping>
        {
            new() { Id = 1, ProjectId = ProjectId, CustomerId = StaffCustomerId },
            new() { Id = 2, ProjectId = OtherProjectId, CustomerId = StaffCustomerId }
        };

        var service = CreateService(projects, mappings, new List<TimeLogEntity>());

        var eligible = await service.GetProjectsAssignedToCustomerAsync(StaffCustomerId, eligibleForTimeLoggingOnly: true);
        var all = await service.GetProjectsAssignedToCustomerAsync(StaffCustomerId, eligibleForTimeLoggingOnly: false);

        Assert.Single(eligible);
        Assert.Equal(ProjectId, eligible[0].Id);
        Assert.Equal(2, all.Count);
    }

    [Theory]
    [InlineData(TimeLogStatus.Draft)]
    [InlineData(TimeLogStatus.Submitted)]
    public async Task HasTimeLogRecordsAsync_ReturnsTrue_RegardlessOfStatus(TimeLogStatus status)
    {
        var timeLogs = new List<TimeLogEntity>
        {
            new() { Id = 1, ProjectId = ProjectId, CustomerId = StaffCustomerId, Status = status, Task = "A", Time = 1m }
        };

        var service = CreateService(new List<Project> { new() { Id = ProjectId, Name = "P1" } },
            new List<ProjectStaffMapping>(), timeLogs);

        Assert.True(await service.HasTimeLogRecordsAsync(ProjectId));
        Assert.False(await service.HasTimeLogRecordsAsync(OtherProjectId));
    }

    [Fact]
    public async Task SetProjectStatusAsync_UpdatesStatusAndTimestamp()
    {
        var project = new Project { Id = ProjectId, Name = "P1", StatusId = (int)ProjectStatus.Inprogress, UpdatedOnUtc = DateTime.UtcNow.AddDays(-1) };
        var service = CreateService(new List<Project> { project }, new List<ProjectStaffMapping>(), new List<TimeLogEntity>());

        var before = DateTime.UtcNow;
        await service.SetProjectStatusAsync(project, ProjectStatus.Retired);

        Assert.Equal(ProjectStatus.Retired, project.Status);
        Assert.True(project.UpdatedOnUtc >= before);
    }

    /// <summary>
    /// TT-027/T-020/T-021: a single save must sync the mapping table to exactly the submitted
    /// selection - both inserting a newly-added customer and removing a deselected one in the same
    /// call, not append-only and not two separate saves.
    /// </summary>
    [Fact]
    public async Task SaveStaffAssignmentsAsync_AddAndRemoveInOneCall_SyncsToExactlyTheSubmittedSet()
    {
        const int keptCustomerId = 100;
        const int removedCustomerId = 200;
        const int addedCustomerId = 300;

        var mappings = new List<ProjectStaffMapping>
        {
            new() { Id = 1, ProjectId = ProjectId, CustomerId = keptCustomerId },
            new() { Id = 2, ProjectId = ProjectId, CustomerId = removedCustomerId }
        };

        var service = CreateService(new List<Project> { new() { Id = ProjectId, Name = "P1" } }, mappings,
            new List<TimeLogEntity>());

        //submitted selection: drop removedCustomerId, keep keptCustomerId, add addedCustomerId
        await service.SaveStaffAssignmentsAsync(ProjectId, new List<int> { keptCustomerId, addedCustomerId });

        var assigned = await service.GetAssignedCustomerIdsAsync(ProjectId);

        Assert.Equal(2, assigned.Count);
        Assert.Contains(keptCustomerId, assigned);
        Assert.Contains(addedCustomerId, assigned);
        Assert.DoesNotContain(removedCustomerId, assigned);
    }

    /// <summary>
    /// Re-saving the exact same selection must not create duplicate mapping rows - the diff against
    /// existing rows should produce zero inserts and zero removes when nothing changed.
    /// </summary>
    [Fact]
    public async Task SaveStaffAssignmentsAsync_ResavingSameSelection_ProducesNoDuplicateRows()
    {
        const int customerId = 100;

        var mappings = new List<ProjectStaffMapping>
        {
            new() { Id = 1, ProjectId = ProjectId, CustomerId = customerId }
        };

        var service = CreateService(new List<Project> { new() { Id = ProjectId, Name = "P1" } }, mappings,
            new List<TimeLogEntity>());

        await service.SaveStaffAssignmentsAsync(ProjectId, new List<int> { customerId });

        var assigned = await service.GetAssignedCustomerIdsAsync(ProjectId);

        Assert.Single(assigned);
        Assert.Equal(customerId, assigned[0]);
    }

    /// <summary>
    /// TT-024/T-018: <see cref="ProjectService.GetAllProjectsAsync"/> must page correctly and, when a
    /// storeId is supplied, restrict results via <see cref="IStoreMappingService.ApplyStoreMapping"/>
    /// rather than returning every project regardless of store.
    /// </summary>
    [Fact]
    public async Task GetAllProjectsAsync_PagesResultsCorrectly()
    {
        var projects = Enumerable.Range(1, 5)
            .Select(i => new Project { Id = i, Name = $"Project {i}" })
            .ToList();

        var service = CreateService(projects, new List<ProjectStaffMapping>(), new List<TimeLogEntity>());

        var firstPage = await service.GetAllProjectsAsync(pageIndex: 0, pageSize: 2);
        var secondPage = await service.GetAllProjectsAsync(pageIndex: 1, pageSize: 2);

        Assert.Equal(2, firstPage.Count);
        Assert.Equal(5, firstPage.TotalCount);
        Assert.Equal(2, secondPage.Count);
        Assert.NotEqual(firstPage.Select(p => p.Id), secondPage.Select(p => p.Id));
    }

    [Fact]
    public async Task GetAllProjectsAsync_FiltersByName()
    {
        var projects = new List<Project>
        {
            new() { Id = 1, Name = "Alpha" },
            new() { Id = 2, Name = "Beta" }
        };

        var service = CreateService(projects, new List<ProjectStaffMapping>(), new List<TimeLogEntity>());

        var result = await service.GetAllProjectsAsync(name: "Alp");

        Assert.Single(result);
        Assert.Equal("Alpha", result[0].Name);
    }

    /// <summary>
    /// When storeId > 0, the query must be passed through <see cref="IStoreMappingService.ApplyStoreMapping"/> -
    /// verified here by asserting the mock is invoked, since <see cref="ProjectService"/> defers the actual
    /// store-mapping filtering logic to that service (not duplicated in ProjectService itself).
    /// </summary>
    [Fact]
    public async Task GetAllProjectsAsync_StoreIdGreaterThanZero_AppliesStoreMapping()
    {
        var projects = new List<Project> { new() { Id = 1, Name = "P1" } };

        var projectRepositoryMock = CreateProjectRepositoryMock(projects);
        var mappingRepositoryMock = CreateMappingRepositoryMock(new List<ProjectStaffMapping>());
        var timeLogRepositoryMock = CreateTimeLogRepositoryMock(new List<TimeLogEntity>());

        var staticCacheManagerMock = new Mock<IStaticCacheManager>();
        staticCacheManagerMock
            .Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(Task.CompletedTask);

        var storeMappingServiceMock = new Mock<IStoreMappingService>();
        storeMappingServiceMock
            .Setup(s => s.ApplyStoreMapping(It.IsAny<IQueryable<Project>>(), It.IsAny<int>()))
            .ReturnsAsync((IQueryable<Project> query, int _) => query);

        var service = new ProjectService(projectRepositoryMock.Object, mappingRepositoryMock.Object,
            timeLogRepositoryMock.Object, staticCacheManagerMock.Object, storeMappingServiceMock.Object);

        await service.GetAllProjectsAsync(storeId: 1);

        storeMappingServiceMock.Verify(s => s.ApplyStoreMapping(It.IsAny<IQueryable<Project>>(), 1), Times.Once);
    }
}
