using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.TimeLog.Domain;
using Nop.Plugin.Misc.TimeLog.Infrastructure;
using Nop.Plugin.Misc.TimeLog.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.TimeLog;

/// <summary>
/// Represents the Time Log plugin.
/// </summary>
/// <remarks>
/// TT-016 lifecycle notes (see docs/time-log/tickets/tickets.md for the full audit trail):
/// - Schema migration is NOT invoked here. In this 5.00 codebase, <c>Nop.Services.Plugins.PluginService</c>
///   calls <c>IMigrationManager.ApplyUpMigrations(assembly, MigrationProcessType.Installation)</c>
///   immediately BEFORE calling <see cref="InstallAsync"/>, and <c>IMigrationManager.ApplyDownMigrations(assembly)</c>
///   immediately AFTER <see cref="UninstallAsync"/> returns (confirmed in
///   <c>Nop.Services.Plugins.PluginService.InstallPluginsAsync</c>/<c>UninstallPluginsAsync</c>). The
///   plugin itself never triggers its own migration - Data/Migrations/SchemaMigration.cs's
///   AutoReversingMigration.Down() is invoked automatically by the framework and needs no explicit call.
/// - Permission RECORD installation (PermissionRecord rows + role mappings) is also framework-driven, but
///   at a different point: <c>Nop.Services.Security.PermissionService.InsertPermissionsAsync()</c> is called
///   from <c>Nop.Web.Framework.Infrastructure.AppStartedConsumer</c> on every application start, which
///   auto-discovers every <see cref="IPermissionConfigManager"/> implementation via ITypeFinder (including
///   <see cref="PermissionProvider"/>) and inserts any permission system name not yet present - installing
///   a plugin in nopCommerce always triggers an application restart, so this fires immediately after install
///   with no explicit call needed here (this is exactly the pattern Nop.Plugin.Misc.RFQ.RfqPlugin follows -
///   its InstallAsync never calls anything permission-related either).
/// - This DOES create a duplicate-role-creation risk flagged by TT-009: PermissionService.InstallPermissionsAsync's
///   internal per-config loop find-or-creates each config's DefaultCustomerRoles entry by system name, and if
///   it does not find a role it creates one with <c>Name = systemRoleName</c> (the raw system name, e.g.
///   "TimeLogStaff"/"TimeLogManager" - not a friendly display name). Resolution: InstallAsync below calls
///   PermissionProvider's own <c>GetOrCreateStaffRoleAsync</c>/<c>GetOrCreateManagerRoleAsync</c> helpers
///   FIRST (creating the roles with friendly "Staff"/"Manager" names if missing), so that when
///   InsertPermissionsAsync runs on the post-install restart, <c>GetCustomerRoleBySystemNameAsync</c> already
///   finds both roles and the framework's own role-creation branch becomes a no-op - only one code path ever
///   actually creates the role.
/// - Staff role: verified against this codebase - there is NO built-in "Staff" CustomerRole shipped with core
///   nopCommerce 5.00 (core ships only Administrators/Registered/Guests/Vendors/ForumModerators). The design
///   doc's assumption that "Staff" is a standard role was wrong; this plugin's own find-or-create logic
///   (<see cref="PermissionProvider.GetOrCreateStaffRoleAsync"/>) is therefore required, exactly like the
///   Manager role, and the SAME safe-deletion logic (created-by-plugin flag + zero-assignment check) is
///   applied to Staff on uninstall below, not just Manager.
/// </remarks>
public class TimeLogPlugin : BasePlugin, IMiscPlugin
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly IProjectService _projectService;
    private readonly IRepository<Project> _projectRepository;

    #endregion

    #region Ctor

    public TimeLogPlugin(ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IProjectService projectService,
        IRepository<Project> projectRepository)
    {
        _customerService = customerService;
        _genericAttributeService = genericAttributeService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _projectService = projectService;
        _projectRepository = projectRepository;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Seeds the 3 default Active projects, but only if the Project table is currently empty -
    /// avoids duplicating seed rows on a repeat install after a partial-failure scenario.
    /// </summary>
    private async Task SeedDefaultProjectsAsync()
    {
        var projectExists = await _projectRepository.Table.AnyAsync();
        if (projectExists)
            return;

        foreach (var name in new[] { "General", "Internal", "Client Support" })
        {
            await _projectService.InsertProjectAsync(new Project
            {
                Name = name,
                Active = true
            });
        }
    }

    /// <summary>
    /// Deletes a customer role on uninstall only if this plugin created it during install
    /// (tracked via <paramref name="createdByPluginAttributeKey"/>) and zero customers are
    /// currently assigned to it - otherwise the role is left untouched.
    /// </summary>
    private async Task<string> SafelyDeleteRoleIfPluginCreatedAsync(string roleSystemName, string createdByPluginAttributeKey, string roleLabel)
    {
        var role = await _customerService.GetCustomerRoleBySystemNameAsync(roleSystemName);
        if (role is null)
            return $"{roleLabel} role does not exist - nothing to remove.";

        var createdByPlugin = await _genericAttributeService.GetAttributeAsync<bool>(role, createdByPluginAttributeKey);
        if (!createdByPlugin)
            return $"{roleLabel} role was not created by this plugin (pre-existing) - left untouched.";

        var assignedCustomers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { role.Id },
            pageIndex: 0, pageSize: 1, getOnlyTotalCount: true);

        if (assignedCustomers.TotalCount > 0)
            return $"{roleLabel} role was created by this plugin but has {assignedCustomers.TotalCount} customer(s) still assigned - left untouched.";

        await _customerService.DeleteCustomerRoleAsync(role);
        return $"{roleLabel} role was created by this plugin and had zero assigned customers - deleted.";
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the plugin's full set of locale resources (install- and update-time; the dictionary is
    /// shared so an upgrade from a pre-Phase-2 install seeds exactly the same Phase 2 keys as a fresh
    /// install, via <see cref="UpdateAsync"/> below - <c>AddOrUpdateLocaleResourceAsync</c> is an upsert,
    /// so re-adding pre-existing Phase 1 keys here on update is a safe no-op).
    /// </summary>
    private static Dictionary<string, string> GetLocaleResources()
    {
        return new Dictionary<string, string>
        {
            //admin menu (Infrastructure/AdminMenuManager.cs)
            ["Admin.TimeLog.Menu.TimeLog"] = "Time Log",
            ["Admin.TimeLog.Menu.LogTime"] = "Log Time",
            ["Admin.TimeLog.Menu.TimeLogAll"] = "Time Logs (All Staff)",
            //Phase 2 (TT-030/TT-031): "Project" child menu item, gated by ManageProjects
            ["Admin.TimeLog.Menu.Project"] = "Project",

            //Phase 2 (TT-032..TT-041): Project List grid (Views/Project/List.cshtml)
            ["Admin.TimeLog.Project.List.AddNew"] = "Add new",
            ["Admin.TimeLog.Project.List.Name"] = "Name",
            ["Admin.TimeLog.Project.List.StartDate"] = "Start date",
            ["Admin.TimeLog.Project.List.EndDate"] = "End date",
            ["Admin.TimeLog.Project.List.Status"] = "Status",
            ["Admin.TimeLog.Project.List.AssignedStaffCount"] = "Assigned staff",

            //Phase 2: Project Create/Edit form field labels (ProjectModel, Views/Project/_CreateOrUpdate.cshtml)
            ["Admin.TimeLog.Project.Fields.Name"] = "Name",
            ["Admin.TimeLog.Project.Fields.StartDate"] = "Start date",
            ["Admin.TimeLog.Project.Fields.EndDate"] = "End date",
            ["Admin.TimeLog.Project.Fields.Description"] = "Description",
            ["Admin.TimeLog.Project.Fields.Status"] = "Status",
            ["Admin.TimeLog.Project.Fields.LimitedToStores"] = "Limited to stores",
            ["Admin.TimeLog.Project.Fields.AvailableStaff"] = "Available staff",
            ["Admin.TimeLog.Project.Fields.AssignedStaff"] = "Assigned staff",
            ["Admin.TimeLog.Project.Fields.MoveToAssigned"] = "Add to assigned",
            ["Admin.TimeLog.Project.Fields.MoveToAvailable"] = "Remove from assigned",

            //Phase 2: Project field validation (Validators/ProjectValidator.cs)
            ["Admin.TimeLog.Project.Validation.NameRequired"] = "Name is required.",
            ["Admin.TimeLog.Project.Validation.StartDateRequired"] = "Start date is required.",
            ["Admin.TimeLog.Project.Validation.EndDateBeforeStartDate"] = "End date must be on or after the start date.",
            ["Admin.TimeLog.Project.Validation.StatusInvalid"] = "The selected status is not valid.",

            //Phase 2: Project page/notification text (ProjectController)
            ["Admin.TimeLog.Project.Edit.PageTitle"] = "Edit project",
            ["Admin.TimeLog.Project.Added"] = "The new project has been added successfully.",
            ["Admin.TimeLog.Project.Updated"] = "The project has been updated successfully.",
            ["Admin.TimeLog.Project.Deleted"] = "The project has been deleted successfully.",
            ["Admin.TimeLog.Project.Delete.Confirm"] = "Are you sure you want to delete this project?",
            //TT-047: the prior "BlockedHasTimeLogs" key (superseded by BlockedRetired below per TT-045/TT-046)
            //was removed here after a full-repo grep (Views/Controllers/Models/Validators) confirmed zero
            //remaining references anywhere - genuinely dead, not just superseded, so it is not re-added.
            //TT-045/TT-046/T-029: shown (as an informational/warning notification, not an error) when a
            //Delete is redirected into an automatic Retired transition because the project has existing
            //TimeLog records (Draft or Submitted)
            ["Admin.TimeLog.Project.Delete.BlockedRetired"] = "This project has logged time entries; its status has been set to Retired instead of being deleted.",

            //Phase 2: ProjectStatus enum display (ILocalizationService.GetLocalizedEnumAsync) - same
            //"Enums.{Enum's full CLR type name}.{value}" key format as TimeLogStatus above
            ["Enums.Nop.Plugin.Misc.TimeLog.Domain.ProjectStatus.NotStarted"] = "Not Started",
            ["Enums.Nop.Plugin.Misc.TimeLog.Domain.ProjectStatus.Inprogress"] = "In Progress",
            ["Enums.Nop.Plugin.Misc.TimeLog.Domain.ProjectStatus.OnHold"] = "On Hold",
            ["Enums.Nop.Plugin.Misc.TimeLog.Domain.ProjectStatus.Completed"] = "Completed",
            ["Enums.Nop.Plugin.Misc.TimeLog.Domain.ProjectStatus.Cancelled"] = "Cancelled",
            ["Enums.Nop.Plugin.Misc.TimeLog.Domain.ProjectStatus.Retired"] = "Retired",

            //grid/page text (Views/TimeLog/List.cshtml, Views/TimeLogAdmin/List.cshtml)
            ["Admin.TimeLog.AddNew"] = "Add Time Entry",
            ["Admin.TimeLog.Submit.Button"] = "Submit",

            //field labels (TimeLogModel, TimeLogAdminModel, both List.cshtml views)
            ["Admin.TimeLog.Fields.Project"] = "Project",
            ["Admin.TimeLog.Fields.Task"] = "Task",
            ["Admin.TimeLog.Fields.Description"] = "Description",
            ["Admin.TimeLog.Fields.Date"] = "Date",
            ["Admin.TimeLog.Fields.Time"] = "Time",
            ["Admin.TimeLog.Fields.Status"] = "Status",
            ["Admin.TimeLog.Fields.Customer"] = "Customer",
            //ENH-008: single combined icon-only Edit/Delete column header
            ["Admin.TimeLog.Fields.Actions"] = "Actions",

            //filter labels (TimeLogSearchModel, TimeLogAdminSearchModel)
            ["Admin.TimeLog.Filter.DateFrom"] = "Date from",
            ["Admin.TimeLog.Filter.DateTo"] = "Date to",
            ["Admin.TimeLog.Filter.Status"] = "Status",
            ["Admin.TimeLog.Filter.Project"] = "Project",
            ["Admin.TimeLog.Filter.Customer"] = "Customer",

            //field validation (Validators/TimeLogValidator.cs)
            ["Admin.TimeLog.Validation.ProjectRequired"] = "Project is required.",
            ["Admin.TimeLog.Validation.ProjectInvalidOrInactive"] = "The selected project does not exist or is not active.",
            ["Admin.TimeLog.Validation.TaskRequired"] = "Task is required.",
            ["Admin.TimeLog.Validation.TimeOutOfRange"] = "Time must be between 0 and 24 hours.",
            ["Admin.TimeLog.Validation.DateRequired"] = "Date is required.",

            //server-side write-path errors (TimeLogController.TimeLogUpdate/TimeLogDelete,
            //TimeLogService.UpdateTimeLogAsync/DeleteTimeLogAsync/SubmitTimeLogsAsync)
            ["Admin.TimeLog.Validation.RecordNotFound"] = "The record was not found or does not belong to you.",
            ["Admin.TimeLog.Validation.RecordNotEditable"] = "This record can no longer be edited because it has been submitted.",

            //bulk submit per-row failures (TimeLogService.SubmitTimeLogsAsync)
            ["Admin.TimeLog.Submit.RecordNotDraft"] = "This record can no longer be submitted because it is not a Draft entry.",
            ["Admin.TimeLog.Submit.ProjectInactive"] = "The project for this entry is no longer active.",

            //TimeLogStatus enum display (ILocalizationService.GetLocalizedEnumAsync) - real nopCommerce 5.00
            //enum-resource key format is "Enums.{Enum's full CLR type name}.{value}", NOT the design doc's
            //assumed "Admin.TimeLog.Status.*" - confirmed against Nop.Plugin.Misc.RFQ's own enum resource keys
            ["Enums.Nop.Plugin.Misc.TimeLog.Domain.TimeLogStatus.Draft"] = "Draft",
            ["Enums.Nop.Plugin.Misc.TimeLog.Domain.TimeLogStatus.Submitted"] = "Submitted",

            //permission display names - real nopCommerce 5.00 mechanism is a "Security.Permission.{SystemName}"
            //resource (PermissionConfig.Name is the raw fallback string), NOT the design doc's assumed flat
            //"Permission.ManageTimeLog"/"Permission.ManageTimeLogAll" keys - confirmed against
            //Nop.Plugin.Misc.RFQ.RfqPlugin's own "Security.Permission.Misc.RFQ.AccessRFQ.*" resource keys
            [$"Security.Permission.{PermissionProvider.ManageTimeLog}"] = "Admin area. Log and manage own time log entries",
            [$"Security.Permission.{PermissionProvider.ManageTimeLogAll}"] = "Admin area. View time log entries for all staff",
            //Phase 2 (TT-030): ManageProjects permission friendly display name, same resource-key mechanism
            [$"Security.Permission.{PermissionProvider.ManageProjects}"] = "Admin area. Manage projects and staff assignments"
        };
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //schema migration: handled automatically by Nop.Services.Plugins.PluginService before this method
        //runs (ApplyUpMigrations against MigrationProcessType.Installation) - see remarks above, no call needed here.

        //find-or-create the Staff and Manager roles with friendly display names, BEFORE the framework's own
        //PermissionService.InstallPermissionsAsync (triggered by AppStartedConsumer on the post-install restart)
        //has a chance to create them itself with a raw system-name fallback - see remarks above for the
        //full duplicate-creation-risk resolution.
        var (staffRole, staffCreated) = await PermissionProvider.GetOrCreateStaffRoleAsync(_customerService);
        await _genericAttributeService.SaveAttributeAsync(staffRole, NopTimeLogDefaults.StaffRoleCreatedByPluginAttribute, staffCreated);

        var (managerRole, managerCreated) = await PermissionProvider.GetOrCreateManagerRoleAsync(_customerService);
        await _genericAttributeService.SaveAttributeAsync(managerRole, NopTimeLogDefaults.ManagerRoleCreatedByPluginAttribute, managerCreated);

        //Phase 2 (TT-030): find-or-create the Project Manager role the same way, before the framework's own
        //PermissionService.InstallPermissionsAsync gets a chance to create it with a raw system-name fallback
        var (projectManagerRole, projectManagerCreated) = await PermissionProvider.GetOrCreateProjectManagerRoleAsync(_customerService);
        await _genericAttributeService.SaveAttributeAsync(projectManagerRole, NopTimeLogDefaults.ProjectManagerRoleCreatedByPluginAttribute, projectManagerCreated);

        //permission RECORDS/mappings are installed automatically by PermissionService.InsertPermissionsAsync
        //(called from AppStartedConsumer on application start; installing a plugin always triggers a restart,
        //so PermissionProvider - already discoverable via IPermissionConfigManager - is picked up with no
        //explicit call needed here). This mirrors Nop.Plugin.Misc.RFQ.RfqPlugin.InstallAsync exactly.

        //seed 3 Active default projects, only if the Project table is currently empty
        await SeedDefaultProjectsAsync();

        //locales (TT-015) - authoritative key list compiled from actual @T(...)/GetResourceAsync/
        //NopResourceDisplayName/GetLocalizedEnumAsync usages across the plugin's controllers, views,
        //validator and admin menu manager - see tickets.md TT-015 Notes for the full audit
        await _localizationService.AddOrUpdateLocaleResourceAsync(GetLocaleResources());

        await base.InstallAsync();
    }

    /// <summary>
    /// Update the plugin - runs for stores that already had Phase 1 installed before Phase 2 shipped.
    /// </summary>
    /// <remarks>
    /// Bumping <c>plugin.json</c>'s Version (1.0 -> 1.1) is what makes
    /// <c>Nop.Services.Plugins.PluginService.UpdatePluginsAsync</c> notice a change at all: it compares the
    /// stored installed-plugin version against plugin.json's version, and only then does it (a) apply pending
    /// <see cref="MigrationProcessType.Update"/> migrations for this assembly - picking up
    /// <c>Data/Migrations/AddProjectManagementSchemaMigration.cs</c> - and (b) call this method. Without the
    /// version bump neither the schema migration nor this seeding would ever run against an already-installed
    /// store, which is exactly the bug this override fixes: the Phase 2 columns/table would exist only in code,
    /// never in an existing store's database, and the Project Manager role/ManageProjects permission's friendly
    /// name + locale resources would never get seeded either.
    /// </remarks>
    /// <param name="currentVersion">Current version of plugin</param>
    /// <param name="targetVersion">New version of plugin</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        //same find-or-create used by InstallAsync, so an upgrade gets the same friendly-name/duplicate-role
        //protection as a fresh install - see the class-level remarks for why this ordering matters relative
        //to PermissionService.InsertPermissionsAsync's own raw-system-name fallback.
        var (projectManagerRole, projectManagerCreated) = await PermissionProvider.GetOrCreateProjectManagerRoleAsync(_customerService);
        await _genericAttributeService.SaveAttributeAsync(projectManagerRole, NopTimeLogDefaults.ProjectManagerRoleCreatedByPluginAttribute, projectManagerCreated);

        //upsert the full locale resource set (safe no-op for pre-existing Phase 1 keys, seeds every new
        //Phase 2 key for a store that only ever ran InstallAsync against the Phase 1 version)
        await _localizationService.AddOrUpdateLocaleResourceAsync(GetLocaleResources());

        await base.UpdateAsync(currentVersion, targetVersion);
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //explicitly remove our two permission records (mapping + localized name + record) - unlike
        //installation, there is no framework-automatic uninstall counterpart to AppStartedConsumer's
        //InsertPermissionsAsync, so this must be done here (matches Nop.Plugin.Misc.RFQ.RfqPlugin.UninstallAsync).
        await _permissionService.DeletePermissionAsync(PermissionProvider.ManageTimeLog);
        await _permissionService.DeletePermissionAsync(PermissionProvider.ManageTimeLogAll);
        //Phase 2 (TT-030): symmetric removal of the ManageProjects permission record
        await _permissionService.DeletePermissionAsync(PermissionProvider.ManageProjects);

        //conditionally remove the Staff/Manager/Project Manager roles - ONLY if this plugin created them
        //during install AND zero customers are currently assigned; otherwise leave them untouched
        //(security/data-integrity sensitive - never delete a pre-existing or in-use role).
        //See SafelyDeleteRoleIfPluginCreatedAsync.
        await SafelyDeleteRoleIfPluginCreatedAsync(NopTimeLogDefaults.StaffRoleSystemName,
            NopTimeLogDefaults.StaffRoleCreatedByPluginAttribute, "Staff");
        await SafelyDeleteRoleIfPluginCreatedAsync(NopTimeLogDefaults.ManagerRoleSystemName,
            NopTimeLogDefaults.ManagerRoleCreatedByPluginAttribute, "Manager");
        await SafelyDeleteRoleIfPluginCreatedAsync(NopTimeLogDefaults.ProjectManagerRoleSystemName,
            NopTimeLogDefaults.ProjectManagerRoleCreatedByPluginAttribute, "Project Manager");

        //locales (TT-015, extended by TT-030) - symmetric removal of everything seeded in InstallAsync above
        await _localizationService.DeleteLocaleResourcesAsync("Admin.TimeLog");
        await _localizationService.DeleteLocaleResourcesAsync("Enums.Nop.Plugin.Misc.TimeLog.Domain.TimeLogStatus");
        //Phase 2 (TT-032..TT-041): symmetric removal of the ProjectStatus enum resources - a distinct
        //prefix from TimeLogStatus above, so it needs its own explicit delete call
        await _localizationService.DeleteLocaleResourcesAsync("Enums.Nop.Plugin.Misc.TimeLog.Domain.ProjectStatus");
        await _localizationService.DeleteLocaleResourceAsync($"Security.Permission.{PermissionProvider.ManageTimeLog}");
        await _localizationService.DeleteLocaleResourceAsync($"Security.Permission.{PermissionProvider.ManageTimeLogAll}");
        await _localizationService.DeleteLocaleResourceAsync($"Security.Permission.{PermissionProvider.ManageProjects}");

        //schema teardown: handled automatically by Nop.Services.Plugins.PluginService immediately AFTER this
        //method returns (IMigrationManager.ApplyDownMigrations against this plugin's assembly), which invokes
        //Data/Migrations/SchemaMigration.cs's AutoReversingMigration.Down() and drops both the Project and
        //TimeLog tables (incidentally removing the seed rows) - no explicit call needed here.

        await base.UninstallAsync();
    }

    #endregion
}
