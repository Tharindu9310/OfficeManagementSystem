using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.TimeLog.Domain;
using Nop.Plugin.Misc.TimeLog.Infrastructure;
using Nop.Plugin.Misc.TimeLog.Models.Admin;
using Nop.Plugin.Misc.TimeLog.Services;
using Nop.Plugin.Misc.TimeLog.Validators;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.TimeLog.Controllers;

/// <summary>
/// Represents the Project admin controller (Phase 2 - TT-033/TT-035/TT-038/TT-039/TT-041).
/// Standard nopCommerce admin create/edit form pattern (not the Phase 1 inline-grid style), since
/// the staff dual-listbox needs more room than a grid cell (design §3.4). Per requirements §6
/// Decision 1, the Edit screen doubles as the details view - there is no separate Details action.
/// Every action is gated by <see cref="PermissionProvider.ManageProjects"/>, independent of
/// <see cref="PermissionProvider.ManageTimeLog"/>/<see cref="PermissionProvider.ManageTimeLogAll"/>.
/// </summary>
[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class ProjectController : BasePluginController
{
    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IProjectService _projectService;
    protected readonly IStoreContext _storeContext;
    protected readonly IStoreMappingService _storeMappingService;
    protected readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    protected readonly ProjectValidator _projectValidator;

    #endregion

    #region Ctor

    public ProjectController(ICustomerService customerService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IProjectService projectService,
        IStoreContext storeContext,
        IStoreMappingService storeMappingService,
        IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
        ProjectValidator projectValidator)
    {
        _customerService = customerService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _projectService = projectService;
        _storeContext = storeContext;
        _storeMappingService = storeMappingService;
        _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        _projectValidator = projectValidator;
    }

    #endregion

    #region Utilities

    private async Task<ProjectModel> PrepareProjectModelAsync(Project project)
    {
        var model = new ProjectModel
        {
            Id = project.Id,
            Name = project.Name,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Description = project.Description,
            StatusId = project.StatusId,
            StatusName = await _localizationService.GetLocalizedEnumAsync(project.Status),
            LimitedToStores = project.LimitedToStores,
            AssignedStaffCount = await _projectService.GetAssignedStaffCountAsync(project.Id)
        };

        return model;
    }

    /// <summary>
    /// Populates the Status dropdown (all six <see cref="ProjectStatus"/> values - status is
    /// free-form per requirements §6 Decision 3, no transition restrictions)
    /// </summary>
    private async Task PrepareStatusSelectListAsync(ProjectModel model)
    {
        model.AvailableStatuses.Clear();

        foreach (ProjectStatus status in Enum.GetValues(typeof(ProjectStatus)))
        {
            model.AvailableStatuses.Add(new SelectListItem
            {
                Value = ((int)status).ToString(),
                Text = await _localizationService.GetLocalizedEnumAsync(status)
            });
        }
    }

    /// <summary>
    /// Populates the staff dual-listbox (TT-036/TT-037/TT-039) - "available" is all Staff-role
    /// customers not currently selected, "assigned" is the currently selected set. Filtered by
    /// Staff-role membership only (not by excluding the current user - a user may hold both the
    /// Project Manager and Staff roles, per requirements §2.4/design §4.2).
    /// </summary>
    private async Task PrepareStaffPickerListsAsync(ProjectModel model, IList<int> assignedCustomerIds)
    {
        model.SelectedCustomerIds = assignedCustomerIds ?? new List<int>();
        model.AvailableStaff.Clear();
        model.AssignedStaff.Clear();

        var staffRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopTimeLogDefaults.StaffRoleSystemName);

        if (staffRole == null)
            return;

        var staffCustomers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { staffRole.Id });
        var assignedSet = model.SelectedCustomerIds.ToHashSet();

        foreach (var customer in staffCustomers)
        {
            var item = new SelectListItem { Value = customer.Id.ToString(), Text = customer.Email ?? customer.Username };

            if (assignedSet.Contains(customer.Id))
                model.AssignedStaff.Add(item);
            else
                model.AvailableStaff.Add(item);
        }
    }

    private async Task<Dictionary<string, string>> ValidateProjectAsync(Project project)
    {
        var validationResult = await _projectValidator.ValidateAsync(project);

        if (validationResult.IsValid)
            return null;

        return validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.First().ErrorMessage);
    }

    private void AddModelErrors(Dictionary<string, string> fieldErrors)
    {
        foreach (var (field, message) in fieldErrors)
            ModelState.AddModelError(field, message);
    }

    #endregion

    #region Methods

    [CheckPermission(PermissionProvider.ManageProjects)]
    public virtual Task<IActionResult> List()
    {
        var searchModel = new ProjectSearchModel();
        searchModel.SetGridPageSize();

        return Task.FromResult<IActionResult>(View("~/Plugins/Misc.TimeLog/Views/Project/List.cshtml", searchModel));
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.ManageProjects)]
    public virtual async Task<IActionResult> ProjectList(ProjectSearchModel searchModel)
    {
        //T-018/AC-P2-2: store-scoped, paged, server-side page-size clamp (NopTimeLogDefaults.MaxPageSize)
        var pageSize = Math.Min(searchModel.PageSize, NopTimeLogDefaults.MaxPageSize);

        //Store-context fix (follow-up to TT-033/T-018): resolve the current admin store context and pass
        //it through so the grid respects Project's IStoreMappingSupported store-mapping (Decision 5),
        //matching the pattern core admin controllers use (IStoreContext.GetCurrentStoreAsync()).
        var store = await _storeContext.GetCurrentStoreAsync();

        var projects = await _projectService.GetAllProjectsAsync(searchModel.SearchName,
            storeId: store.Id, pageIndex: searchModel.Page - 1, pageSize: pageSize);

        var model = await new ProjectListModel().PrepareToGridAsync(searchModel, projects, () =>
        {
            return projects.SelectAwait(async project => await PrepareProjectModelAsync(project));
        });

        return Json(model);
    }

    [CheckPermission(PermissionProvider.ManageProjects)]
    public virtual async Task<IActionResult> Create()
    {
        var model = new ProjectModel
        {
            StartDate = DateTime.UtcNow.Date,
            StatusId = (int)ProjectStatus.NotStarted
        };

        await PrepareStatusSelectListAsync(model);
        await PrepareStaffPickerListsAsync(model, new List<int>());
        await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model);

        return View("~/Plugins/Misc.TimeLog/Views/Project/Create.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(PermissionProvider.ManageProjects)]
    public virtual async Task<IActionResult> Create(ProjectModel model, bool continueEditing)
    {
        var project = new Project
        {
            Name = model.Name,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Description = model.Description,
            StatusId = model.StatusId,
            Active = true,
            LimitedToStores = model.LimitedToStores,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        //T-023/TT-041: ModelState validated first, then the centralized FluentValidation rules
        //(server-side, regardless of client-side state - rejects even a tampered out-of-range Status)
        var fieldErrors = ModelState.IsValid ? await ValidateProjectAsync(project) : null;

        if (fieldErrors != null)
            AddModelErrors(fieldErrors);

        if (ModelState.IsValid && fieldErrors == null)
        {
            await _projectService.InsertProjectAsync(project);

            //TT-037: sync staff assignments (empty selection is valid - T-021/AC-P2-4.4)
            await _projectService.SaveStaffAssignmentsAsync(project.Id, model.SelectedCustomerIds);

            //Decision 5: store mapping
            await _storeMappingService.SaveStoreMappingsAsync(project, model.SelectedStoreIds);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.TimeLog.Project.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = project.Id });
        }

        //redisplay form
        await PrepareStatusSelectListAsync(model);
        await PrepareStaffPickerListsAsync(model, model.SelectedCustomerIds);
        await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model);

        return View("~/Plugins/Misc.TimeLog/Views/Project/Create.cshtml", model);
    }

    [CheckPermission(PermissionProvider.ManageProjects)]
    public virtual async Task<IActionResult> Edit(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);

        if (project == null)
            return RedirectToAction("List");

        //T-022/AC-P2-5: this screen doubles as the details view - all fields plus assigned staff
        var model = await PrepareProjectModelAsync(project);

        await PrepareStatusSelectListAsync(model);

        var assignedCustomerIds = await _projectService.GetAssignedCustomerIdsAsync(project.Id);
        await PrepareStaffPickerListsAsync(model, assignedCustomerIds);

        await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, project, false);

        return View("~/Plugins/Misc.TimeLog/Views/Project/Edit.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(PermissionProvider.ManageProjects)]
    public virtual async Task<IActionResult> Edit(ProjectModel model, bool continueEditing)
    {
        var project = await _projectService.GetProjectByIdAsync(model.Id);

        if (project == null)
            return RedirectToAction("List");

        project.Name = model.Name;
        project.StartDate = model.StartDate;
        project.EndDate = model.EndDate;
        project.Description = model.Description;
        project.StatusId = model.StatusId;
        project.LimitedToStores = model.LimitedToStores;
        project.UpdatedOnUtc = DateTime.UtcNow;

        var fieldErrors = ModelState.IsValid ? await ValidateProjectAsync(project) : null;

        if (fieldErrors != null)
            AddModelErrors(fieldErrors);

        if (ModelState.IsValid && fieldErrors == null)
        {
            await _projectService.UpdateProjectAsync(project);

            //TT-037/T-020: sync staff assignments to exactly the submitted selection
            await _projectService.SaveStaffAssignmentsAsync(project.Id, model.SelectedCustomerIds);

            await _storeMappingService.SaveStoreMappingsAsync(project, model.SelectedStoreIds);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.TimeLog.Project.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = project.Id });
        }

        //redisplay form
        model.Id = project.Id;
        await PrepareStatusSelectListAsync(model);
        await PrepareStaffPickerListsAsync(model, model.SelectedCustomerIds);
        await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, project, true);

        return View("~/Plugins/Misc.TimeLog/Views/Project/Edit.cshtml", model);
    }

    /// <summary>
    /// Deletes a project (TT-045/TT-046, upgrading TT-041's plain guarded delete). Per requirements
    /// §2.7/§6 Decision 2 and design §4.5: if <see cref="IProjectService.HasTimeLogRecordsAsync"/> is
    /// true (any TimeLog row, Draft or Submitted), the project is NOT blocked with an error - instead
    /// its Status is automatically flipped to <see cref="ProjectStatus.Retired"/> and the admin is
    /// informed the project was retired instead of deleted. Only a project with zero TimeLog rows is
    /// hard-deleted (which also removes its ProjectStaffMapping rows, in <see cref="IProjectService.DeleteProjectAsync"/>).
    /// </summary>
    [HttpPost]
    [CheckPermission(PermissionProvider.ManageProjects)]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);

        if (project == null)
            return RedirectToAction("List");

        if (await _projectService.HasTimeLogRecordsAsync(id))
        {
            //AC-P2-10.3/.4: automatic Retired transition in place of deletion - a system-driven status
            //change, not a user-initiated one (requirements §2.8 Decision 3 carve-out) - then inform,
            //never a plain error, since the project WAS successfully acted upon.
            await _projectService.SetProjectStatusAsync(project, ProjectStatus.Retired);

            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.TimeLog.Project.Delete.BlockedRetired"));

            return RedirectToAction("Edit", new { id });
        }

        await _projectService.DeleteProjectAsync(project);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.TimeLog.Project.Deleted"));

        return RedirectToAction("List");
    }

    #endregion
}
