using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.TimeLog.Domain;
using Nop.Plugin.Misc.TimeLog.Infrastructure;
using Nop.Plugin.Misc.TimeLog.Models.Admin;
using Nop.Plugin.Misc.TimeLog.Services;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using TimeLogEntity = Nop.Plugin.Misc.TimeLog.Domain.TimeLog;

namespace Nop.Plugin.Misc.TimeLog.Controllers;

/// <summary>
/// Represents the manager oversight Time Log admin controller (TT-012) - read-only by omission.
/// This controller deliberately exposes ONLY <see cref="List"/> and <see cref="TimeLogList"/> -
/// no insert/update/delete/submit action exists anywhere in this class, so oversight cannot mutate
/// another staff member's records even if a future change to the view accidentally posts to it.
/// Gated exclusively by <see cref="PermissionProvider.ManageTimeLogAll"/> - a Staff-only user
/// (holding only <see cref="PermissionProvider.ManageTimeLog"/>) is rejected by <see cref="CheckPermissionAttribute"/>
/// at the controller/action level even via a direct URL, per Nop.Plugin.Misc.RFQ.Controllers.RfqAdminController's
/// per-action [CheckPermission] pattern.
/// </summary>
[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class TimeLogAdminController : BasePluginController
{
    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IProjectService _projectService;
    protected readonly ITimeLogService _timeLogService;

    #endregion

    #region Ctor

    public TimeLogAdminController(ICustomerService customerService,
        ILocalizationService localizationService,
        IProjectService projectService,
        ITimeLogService timeLogService)
    {
        _customerService = customerService;
        _localizationService = localizationService;
        _projectService = projectService;
        _timeLogService = timeLogService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Converts a decimal hours value to an exact HH:mm string - see TimeLogController's identical
    /// helper / Domain.TimeLog.Time for the round-trip rationale.
    /// </summary>
    private static string ToTimeDisplay(decimal hours)
    {
        var totalMinutes = (int)Math.Round(hours * 60m, MidpointRounding.AwayFromZero);
        var span = TimeSpan.FromMinutes(totalMinutes);

        return $"{(int)span.TotalHours:D2}:{span.Minutes:D2}";
    }

    private async Task<TimeLogAdminModel> PrepareTimeLogAdminModelAsync(TimeLogEntity timeLog)
    {
        var project = await _projectService.GetProjectByIdAsync(timeLog.ProjectId);
        var customer = await _customerService.GetCustomerByIdAsync(timeLog.CustomerId);

        return new TimeLogAdminModel
        {
            Id = timeLog.Id,
            CustomerId = timeLog.CustomerId,
            CustomerName = customer?.Email ?? customer?.Username,
            ProjectId = timeLog.ProjectId,
            ProjectName = project?.Name,
            Task = timeLog.Task,
            Description = timeLog.Description,
            Date = timeLog.Date,
            Time = timeLog.Time,
            TimeDisplay = ToTimeDisplay(timeLog.Time),
            StatusId = timeLog.StatusId,
            StatusName = await _localizationService.GetLocalizedEnumAsync(timeLog.Status)
        };
    }

    private async Task PrepareFilterSelectListsAsync(TimeLogAdminSearchModel searchModel)
    {
        searchModel.AvailableStatuses.Clear();
        searchModel.AvailableStatuses.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.Common.All") });
        searchModel.AvailableStatuses.Add(new SelectListItem { Value = ((int)TimeLogStatus.Draft + 1).ToString(), Text = await _localizationService.GetLocalizedEnumAsync(TimeLogStatus.Draft) });
        searchModel.AvailableStatuses.Add(new SelectListItem { Value = ((int)TimeLogStatus.Submitted + 1).ToString(), Text = await _localizationService.GetLocalizedEnumAsync(TimeLogStatus.Submitted) });

        searchModel.AvailableProjects.Clear();
        searchModel.AvailableProjects.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.Common.All") });

        var activeProjects = await _projectService.GetAllActiveProjectsAsync();

        foreach (var project in activeProjects)
            searchModel.AvailableProjects.Add(new SelectListItem { Value = project.Id.ToString(), Text = project.Name });

        //simple customer id filter (kept deliberately simple per TT-012 scope - a full customer-picker
        //popup, if wanted, is a TT-014 views concern): staff-role customers only
        searchModel.AvailableCustomers.Clear();
        searchModel.AvailableCustomers.Add(new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.Common.All") });

        var staffRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopTimeLogDefaults.StaffRoleSystemName);

        if (staffRole != null)
        {
            var staffCustomers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { staffRole.Id });

            foreach (var customer in staffCustomers)
                searchModel.AvailableCustomers.Add(new SelectListItem { Value = customer.Id.ToString(), Text = customer.Email });
        }
    }

    #endregion

    #region Methods

    [CheckPermission(PermissionProvider.ManageTimeLogAll)]
    public virtual async Task<IActionResult> List()
    {
        var searchModel = new TimeLogAdminSearchModel();
        await PrepareFilterSelectListsAsync(searchModel);
        searchModel.SetGridPageSize();

        return View("~/Plugins/Misc.TimeLog/Views/TimeLogAdmin/List.cshtml", searchModel);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.ManageTimeLogAll)]
    public virtual async Task<IActionResult> TimeLogList(TimeLogAdminSearchModel searchModel)
    {
        //CR-002: this oversight grid only ever shows Submitted records now, per user request - always
        //scoped server-side regardless of any posted StatusId, so a crafted request can't widen it back
        //to Draft/all statuses.
        TimeLogStatus? status = TimeLogStatus.Submitted;
        int? projectId = searchModel.ProjectId > 0 ? searchModel.ProjectId : null;
        int? customerId = searchModel.CustomerId > 0 ? searchModel.CustomerId : null;

        var timeLogs = await _timeLogService.GetAllTimeLogsAsync(customerId,
            searchModel.DateFrom, searchModel.DateTo, status, projectId,
            searchModel.Page - 1, searchModel.PageSize);

        var model = await new TimeLogAdminListModel().PrepareToGridAsync(searchModel, timeLogs, () =>
        {
            return timeLogs.SelectAwait(async timeLog => await PrepareTimeLogAdminModelAsync(timeLog));
        });

        return Json(model);
    }

    #endregion
}
