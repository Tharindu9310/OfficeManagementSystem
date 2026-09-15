using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Misc.TimeLog.Domain;
using Nop.Plugin.Misc.TimeLog.Infrastructure;
using Nop.Plugin.Misc.TimeLog.Models.Admin;
using Nop.Plugin.Misc.TimeLog.Services;
using Nop.Plugin.Misc.TimeLog.Validators;
using Nop.Services.Localization;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using TimeLogEntity = Nop.Plugin.Misc.TimeLog.Domain.TimeLog;

namespace Nop.Plugin.Misc.TimeLog.Controllers;

/// <summary>
/// Represents the self-service Time Log admin controller (TT-011).
/// Every action is individually permission-checked against <see cref="PermissionProvider.ManageTimeLog"/>.
/// <see cref="Domain.TimeLog.CustomerId"/> is always resolved from <see cref="IWorkContext.GetCurrentCustomerAsync"/> -
/// never bound from the posted model - so a row can never be read/written/deleted/submitted on behalf of
/// another customer, matching the real 5.00 pattern confirmed against
/// Nop.Plugin.Misc.RFQ.Controllers.RfqCustomerController (which resolves the current customer the same way).
/// </summary>
[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class TimeLogController : BasePluginController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly IProjectService _projectService;
    protected readonly IStoreContext _storeContext;
    protected readonly ITimeLogService _timeLogService;
    protected readonly TimeLogValidator _timeLogValidator;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public TimeLogController(ILocalizationService localizationService,
        IProjectService projectService,
        IStoreContext storeContext,
        ITimeLogService timeLogService,
        TimeLogValidator timeLogValidator,
        IWorkContext workContext)
    {
        _localizationService = localizationService;
        _projectService = projectService;
        _storeContext = storeContext;
        _timeLogService = timeLogService;
        _timeLogValidator = timeLogValidator;
        _workContext = workContext;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Converts a decimal hours value to an exact HH:mm string by round-tripping through
    /// minutes (round(hours * 60)) rather than formatting the decimal directly - see
    /// Domain.TimeLog.Time for the rationale (non-terminating fractions e.g. 20 minutes).
    /// </summary>
    private static string ToTimeDisplay(decimal hours)
    {
        var totalMinutes = (int)Math.Round(hours * 60m, MidpointRounding.AwayFromZero);
        var span = TimeSpan.FromMinutes(totalMinutes);

        return $"{(int)span.TotalHours:D2}:{span.Minutes:D2}";
    }

    /// <summary>
    /// Parses an HH:mm string back to a decimal hours value. Returns null if the format is invalid -
    /// callers must treat that as a validation failure, never silently default to 0.
    /// </summary>
    private static decimal? FromTimeDisplay(string timeDisplay)
    {
        if (string.IsNullOrWhiteSpace(timeDisplay))
            return null;

        var parts = timeDisplay.Split(':');

        if (parts.Length != 2 || !int.TryParse(parts[0], out var hours) || !int.TryParse(parts[1], out var minutes))
            return null;

        return hours + minutes / 60m;
    }

    private async Task<TimeLogModel> PrepareTimeLogModelAsync(TimeLogEntity timeLog)
    {
        var project = await _projectService.GetProjectByIdAsync(timeLog.ProjectId);
        var isDraft = timeLog.Status == TimeLogStatus.Draft;

        return new TimeLogModel
        {
            Id = timeLog.Id,
            ProjectId = timeLog.ProjectId,
            ProjectName = project?.Name,
            Task = timeLog.Task,
            Description = timeLog.Description,
            Date = timeLog.Date,
            Time = timeLog.Time,
            TimeDisplay = ToTimeDisplay(timeLog.Time),
            StatusId = timeLog.StatusId,
            StatusName = await _localizationService.GetLocalizedEnumAsync(timeLog.Status),
            CanEdit = isDraft,
            CanDelete = isDraft,
            Selectable = isDraft
        };
    }

    /// <summary>
    /// TT-042/TT-043: the single shared eligibility query - assigned to the current customer AND
    /// currently eligible for new time-log entries (<see cref="NopTimeLogDefaults.TimeLoggableProjectStatuses"/>).
    /// Feeds both the insert/edit row's Project dropdown and the toolbar Project filter
    /// (<see cref="PrepareFilterSelectListsAsync"/>), and the server-side re-validation in
    /// <see cref="TimeLogInsert"/>/<see cref="TimeLogUpdate"/> - one query, never a second divergent
    /// check, per design §3.3.
    /// </summary>
    private async Task<IList<Project>> GetEligibleProjectsForCurrentCustomerAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        return await _projectService.GetProjectsAssignedToCustomerAsync(customer.Id,
            eligibleForTimeLoggingOnly: true, storeId: store.Id);
    }

    private async Task PrepareFilterSelectListsAsync(TimeLogSearchModel searchModel)
    {
        searchModel.AvailableStatuses.Clear();
        searchModel.AvailableStatuses.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.Common.All") });
        searchModel.AvailableStatuses.Add(new SelectListItem { Value = ((int)TimeLogStatus.Draft + 1).ToString(), Text = await _localizationService.GetLocalizedEnumAsync(TimeLogStatus.Draft) });
        searchModel.AvailableStatuses.Add(new SelectListItem { Value = ((int)TimeLogStatus.Submitted + 1).ToString(), Text = await _localizationService.GetLocalizedEnumAsync(TimeLogStatus.Submitted) });

        searchModel.AvailableProjects.Clear();
        searchModel.AvailableProjects.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.Common.All") });

        //TT-042/TT-024/TT-025: restricted to projects assigned to the current staff member AND
        //currently eligible for time logging - same call feeds both this toolbar filter and the
        //insert/edit row's Project dropdown (design §3.3), replacing the obsolete
        //GetAllActiveProjectsAsync() "all active projects" source.
        var eligibleProjects = await GetEligibleProjectsForCurrentCustomerAsync();

        foreach (var project in eligibleProjects)
            searchModel.AvailableProjects.Add(new SelectListItem { Value = project.Id.ToString(), Text = project.Name });
    }

    /// <summary>
    /// Runs the centralized <see cref="TimeLogValidator"/> against the given entity and returns a
    /// field-keyed dictionary of error messages (key = the validated property name, e.g. "ProjectId",
    /// "Task", "Time", "Date"), or null when valid. Shared by <see cref="TimeLogInsert"/> and
    /// <see cref="TimeLogUpdate"/> so field rules (AC-10) are enforced identically to
    /// <see cref="Services.TimeLogService.SubmitTimeLogsAsync"/>, never bypassable via insert/update.
    /// ENH-004: field-keyed (not a single joined string) so timelog-grid.js can attach each message to
    /// the specific offending input instead of a single alert/toast.
    /// </summary>
    private async Task<Dictionary<string, string>> ValidateTimeLogAsync(TimeLogEntity timeLog)
    {
        var validationResult = await _timeLogValidator.ValidateAsync(timeLog);

        if (validationResult.IsValid)
            return null;

        //group in case a property has more than one failed rule - keep only the first message per field
        return validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.First().ErrorMessage);
    }

    #endregion

    #region Methods

    [CheckPermission(PermissionProvider.ManageTimeLog)]
    public virtual async Task<IActionResult> List()
    {
        var searchModel = new TimeLogSearchModel();
        await PrepareFilterSelectListsAsync(searchModel);
        searchModel.SetGridPageSize();

        return View("~/Plugins/Misc.TimeLog/Views/TimeLog/List.cshtml", searchModel);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.ManageTimeLog)]
    public virtual async Task<IActionResult> TimeLogList(TimeLogSearchModel searchModel)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        //status filter: 0 = all, otherwise (int)TimeLogStatus + 1 (to keep 0 free for "All")
        TimeLogStatus? status = searchModel.StatusId > 0 ? (TimeLogStatus)(searchModel.StatusId - 1) : null;
        int? projectId = searchModel.ProjectId > 0 ? searchModel.ProjectId : null;

        var timeLogs = await _timeLogService.GetOwnTimeLogsAsync(customer.Id,
            searchModel.DateFrom, searchModel.DateTo, status, projectId,
            searchModel.Page - 1, searchModel.PageSize);

        var model = await new TimeLogListModel().PrepareToGridAsync(searchModel, timeLogs, () =>
        {
            return timeLogs.SelectAwait(async timeLog => await PrepareTimeLogModelAsync(timeLog));
        });

        return Json(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.ManageTimeLog)]
    public virtual async Task<IActionResult> TimeLogInsert(TimeLogModel model)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        var time = FromTimeDisplay(model.TimeDisplay) ?? model.Time;

        var timeLog = new TimeLogEntity
        {
            //CustomerId is ALWAYS taken from the current logged-in customer - never from the posted model
            CustomerId = customer.Id,
            ProjectId = model.ProjectId,
            Task = model.Task,
            Description = model.Description,
            Date = model.Date == default ? DateTime.UtcNow.Date : model.Date,
            Time = time,
            Status = TimeLogStatus.Draft,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        //AC-10 / BUG-001 / ENH-004: field-level validation MUST be enforced here, never only on Submit -
        //an invalid/inactive Project, empty Task, or out-of-range Time must be rejected on insert.
        //Returned as a field-keyed dictionary (not a single joined string) so the UI can highlight the
        //specific offending input instead of showing one alert/toast.
        var fieldErrors = await ValidateTimeLogAsync(timeLog);

        //TT-043/AC-P2-8.3/AC-P2-12: independently re-validate the posted ProjectId against the SAME
        //assigned+eligible query used to build the dropdown - rejects a crafted request that bypasses
        //the UI (an ineligible-status assigned project, or an eligible-status unassigned one).
        if (fieldErrors == null)
        {
            var eligibleProjects = await GetEligibleProjectsForCurrentCustomerAsync();

            if (!eligibleProjects.Any(p => p.Id == timeLog.ProjectId))
                fieldErrors = new Dictionary<string, string>
                {
                    ["ProjectId"] = await _localizationService.GetResourceAsync("Admin.TimeLog.Validation.ProjectInvalidOrInactive")
                };
        }

        if (fieldErrors != null)
            return Json(new { fieldErrors });

        await _timeLogService.InsertTimeLogAsync(timeLog);

        return new NullJsonResult();
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.ManageTimeLog)]
    public virtual async Task<IActionResult> TimeLogUpdate(TimeLogModel model)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        //owner-scoped fetch - a crafted id for another customer's row (or a Submitted row) is
        //rejected below, never distinguished from "not found"
        var timeLog = await _timeLogService.GetOwnTimeLogByIdAsync(model.Id, customer.Id);

        if (timeLog == null)
            return base.ErrorJson(await _localizationService.GetResourceAsync("Admin.TimeLog.Validation.RecordNotFound"));

        if (timeLog.Status != TimeLogStatus.Draft)
            return base.ErrorJson(await _localizationService.GetResourceAsync("Admin.TimeLog.Validation.RecordNotEditable"));

        var time = FromTimeDisplay(model.TimeDisplay) ?? model.Time;

        timeLog.ProjectId = model.ProjectId;
        timeLog.Task = model.Task;
        timeLog.Description = model.Description;
        timeLog.Date = model.Date;
        timeLog.Time = time;
        timeLog.UpdatedOnUtc = DateTime.UtcNow;

        //AC-4.3 / AC-10 / BUG-001 / ENH-004: autosave-on-blur (and the inline Project dropdown) reach this
        //action - the server MUST independently re-validate field rules here, never trusting the
        //client-side regex guard alone. Field-keyed so the grid can flag only the offending cell.
        var fieldErrors = await ValidateTimeLogAsync(timeLog);

        //TT-043/AC-P2-8.3/AC-P2-12: same independent re-validation as TimeLogInsert - a crafted update
        //cannot move a row onto a project the current staff member isn't assigned to, or one that is no
        //longer in an eligible status, even though it would bypass the dropdown UI.
        if (fieldErrors == null)
        {
            var eligibleProjects = await GetEligibleProjectsForCurrentCustomerAsync();

            if (!eligibleProjects.Any(p => p.Id == timeLog.ProjectId))
                fieldErrors = new Dictionary<string, string>
                {
                    ["ProjectId"] = await _localizationService.GetResourceAsync("Admin.TimeLog.Validation.ProjectInvalidOrInactive")
                };
        }

        if (fieldErrors != null)
            return Json(new { fieldErrors });

        await _timeLogService.UpdateTimeLogAsync(timeLog);

        return new NullJsonResult();
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.ManageTimeLog)]
    public virtual async Task<IActionResult> TimeLogDelete(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        var timeLog = await _timeLogService.GetOwnTimeLogByIdAsync(id, customer.Id);

        if (timeLog == null)
            return base.ErrorJson(await _localizationService.GetResourceAsync("Admin.TimeLog.Validation.RecordNotFound"));

        if (timeLog.Status != TimeLogStatus.Draft)
            return base.ErrorJson(await _localizationService.GetResourceAsync("Admin.TimeLog.Validation.RecordNotEditable"));

        await _timeLogService.DeleteTimeLogAsync(timeLog);

        return new NullJsonResult();
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.ManageTimeLog)]
    public virtual async Task<IActionResult> TimeLogSubmit(ICollection<int> selectedIds)
    {
        //empty-selection no-op guard
        if (selectedIds == null || !selectedIds.Any())
            return Json(new { success = true, results = Array.Empty<object>() });

        var customer = await _workContext.GetCurrentCustomerAsync();

        var results = await _timeLogService.SubmitTimeLogsAsync(selectedIds.ToList(), customer.Id);

        return Json(new
        {
            success = true,
            results = results.Select(r => new { r.TimeLogId, r.Success, r.ErrorMessage })
        });
    }

    #endregion
}
