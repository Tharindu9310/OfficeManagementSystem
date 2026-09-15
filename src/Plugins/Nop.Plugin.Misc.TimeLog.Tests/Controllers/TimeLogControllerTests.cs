using Microsoft.AspNetCore.Mvc;
using Moq;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.TimeLog.Controllers;
using Nop.Plugin.Misc.TimeLog.Domain;
using Nop.Plugin.Misc.TimeLog.Models.Admin;
using Nop.Plugin.Misc.TimeLog.Services;
using Nop.Plugin.Misc.TimeLog.Validators;
using Nop.Services.Localization;
using Xunit;

namespace Nop.Plugin.Misc.TimeLog.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="TimeLogController"/>'s TT-043 server-side eligibility re-validation
/// (T-027/AC-P2-8.3/AC-P2-12) - the insert/update actions must independently reject a posted
/// <c>ProjectId</c> that passes field-level validation (project exists and is Active) but is not in
/// the current customer's assigned+eligible set (<see cref="IProjectService.GetProjectsAssignedToCustomerAsync"/>),
/// covering both a crafted unassigned project and a crafted assigned-but-ineligible-status project.
/// </summary>
public class TimeLogControllerTests
{
    private const int CurrentCustomerId = 10;
    private const int CurrentStoreId = 1;
    private const int EligibleAssignedProjectId = 1;
    private const int IneligibleOrUnassignedProjectId = 2;

    private static (TimeLogController controller, Mock<ITimeLogService> timeLogServiceMock, Mock<IProjectService> projectServiceMock)
        CreateController(IList<Project> eligibleAssignedProjects, Project projectForFieldValidation)
    {
        var localizationServiceMock = new Mock<ILocalizationService>();
        localizationServiceMock
            .Setup(x => x.GetResourceAsync(It.IsAny<string>()))
            .ReturnsAsync((string key) => key);

        var projectServiceMock = new Mock<IProjectService>();
        //field-level validation (TimeLogValidator) only cares whether the posted ProjectId's project
        //exists and is Active - it is deliberately unaware of assignment/eligibility (that is TT-043's job)
        projectServiceMock
            .Setup(x => x.GetProjectByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(projectForFieldValidation);
        projectServiceMock
            .Setup(x => x.GetProjectsAssignedToCustomerAsync(CurrentCustomerId, true, CurrentStoreId))
            .ReturnsAsync(eligibleAssignedProjects);

        var workContextMock = new Mock<IWorkContext>();
        workContextMock
            .Setup(x => x.GetCurrentCustomerAsync())
            .ReturnsAsync(new Customer { Id = CurrentCustomerId });

        var storeContextMock = new Mock<IStoreContext>();
        storeContextMock
            .Setup(x => x.GetCurrentStoreAsync())
            .ReturnsAsync(new Store { Id = CurrentStoreId });

        var timeLogServiceMock = new Mock<ITimeLogService>();

        var validator = new TimeLogValidator(localizationServiceMock.Object, projectServiceMock.Object);

        var controller = new TimeLogController(localizationServiceMock.Object, projectServiceMock.Object,
            storeContextMock.Object, timeLogServiceMock.Object, validator, workContextMock.Object);

        return (controller, timeLogServiceMock, projectServiceMock);
    }

    private static TimeLogModel BuildValidModel(int projectId)
    {
        return new TimeLogModel
        {
            ProjectId = projectId,
            Task = "Development",
            Date = DateTime.UtcNow.Date,
            Time = 8m,
            TimeDisplay = "08:00"
        };
    }

    private static Dictionary<string, string> ExtractFieldErrors(IActionResult result)
    {
        var jsonResult = Assert.IsType<JsonResult>(result);
        var fieldErrorsProperty = jsonResult.Value.GetType().GetProperty("fieldErrors");
        Assert.NotNull(fieldErrorsProperty);

        return (Dictionary<string, string>)fieldErrorsProperty.GetValue(jsonResult.Value);
    }

    [Fact]
    public async Task TimeLogInsert_ProjectNotAssignedToCurrentCustomer_RejectsWithProjectIdFieldError()
    {
        //the posted project exists and is Active (passes field validation) but is not in the
        //assigned+eligible set returned for the current customer - a crafted, unassigned ProjectId
        var activeButUnassignedProject = new Project { Id = IneligibleOrUnassignedProjectId, Active = true };
        var (controller, timeLogServiceMock, _) = CreateController(
            eligibleAssignedProjects: new List<Project>(),
            projectForFieldValidation: activeButUnassignedProject);

        var result = await controller.TimeLogInsert(BuildValidModel(IneligibleOrUnassignedProjectId));

        var fieldErrors = ExtractFieldErrors(result);
        Assert.True(fieldErrors.ContainsKey("ProjectId"));
        timeLogServiceMock.Verify(s => s.InsertTimeLogAsync(It.IsAny<Domain.TimeLog>()), Times.Never);
    }

    [Fact]
    public async Task TimeLogInsert_ProjectAssignedButIneligibleStatus_RejectsWithProjectIdFieldError()
    {
        //the posted project is Active (passes field validation, since Active is a separate legacy flag)
        //but GetProjectsAssignedToCustomerAsync(eligibleForTimeLoggingOnly: true) excludes it - e.g. an
        //assigned project that is On Hold - so it never appears in the eligible set
        var activeProject = new Project { Id = IneligibleOrUnassignedProjectId, Active = true, StatusId = (int)ProjectStatus.OnHold };
        var (controller, timeLogServiceMock, _) = CreateController(
            eligibleAssignedProjects: new List<Project> { new() { Id = EligibleAssignedProjectId, Active = true, StatusId = (int)ProjectStatus.Inprogress } },
            projectForFieldValidation: activeProject);

        var result = await controller.TimeLogInsert(BuildValidModel(IneligibleOrUnassignedProjectId));

        var fieldErrors = ExtractFieldErrors(result);
        Assert.True(fieldErrors.ContainsKey("ProjectId"));
        timeLogServiceMock.Verify(s => s.InsertTimeLogAsync(It.IsAny<Domain.TimeLog>()), Times.Never);
    }

    [Fact]
    public async Task TimeLogInsert_ProjectAssignedAndEligible_Succeeds()
    {
        var eligibleProject = new Project { Id = EligibleAssignedProjectId, Active = true, StatusId = (int)ProjectStatus.Inprogress };
        var (controller, timeLogServiceMock, _) = CreateController(
            eligibleAssignedProjects: new List<Project> { eligibleProject },
            projectForFieldValidation: eligibleProject);

        var result = await controller.TimeLogInsert(BuildValidModel(EligibleAssignedProjectId));

        Assert.IsNotType<JsonResult>(result);
        timeLogServiceMock.Verify(s => s.InsertTimeLogAsync(It.Is<Domain.TimeLog>(t =>
            t.ProjectId == EligibleAssignedProjectId && t.CustomerId == CurrentCustomerId)), Times.Once);
    }

    [Fact]
    public async Task TimeLogUpdate_ProjectNotAssignedToCurrentCustomer_RejectsWithProjectIdFieldErrorAndDoesNotPersist()
    {
        var existingTimeLog = new Domain.TimeLog
        {
            Id = 1,
            CustomerId = CurrentCustomerId,
            ProjectId = EligibleAssignedProjectId,
            Task = "Development",
            Date = DateTime.UtcNow.Date,
            Time = 4m,
            Status = TimeLogStatus.Draft
        };

        var activeButUnassignedProject = new Project { Id = IneligibleOrUnassignedProjectId, Active = true };
        var (controller, timeLogServiceMock, _) = CreateController(
            eligibleAssignedProjects: new List<Project> { new() { Id = EligibleAssignedProjectId, Active = true, StatusId = (int)ProjectStatus.Inprogress } },
            projectForFieldValidation: activeButUnassignedProject);

        timeLogServiceMock
            .Setup(s => s.GetOwnTimeLogByIdAsync(1, CurrentCustomerId))
            .ReturnsAsync(existingTimeLog);

        var model = BuildValidModel(IneligibleOrUnassignedProjectId);
        model.Id = 1;

        var result = await controller.TimeLogUpdate(model);

        var fieldErrors = ExtractFieldErrors(result);
        Assert.True(fieldErrors.ContainsKey("ProjectId"));
        timeLogServiceMock.Verify(s => s.UpdateTimeLogAsync(It.IsAny<Domain.TimeLog>()), Times.Never);
    }
}
