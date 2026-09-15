using Nop.Core.Caching;
using Nop.Plugin.Misc.TimeLog.Domain;

namespace Nop.Plugin.Misc.TimeLog;

/// <summary>
/// Represents plugin constants
/// </summary>
public static class NopTimeLogDefaults
{
    /// <summary>
    /// Gets the plugin system name (matches plugin.json's "SystemName")
    /// </summary>
    public const string SystemName = "Misc.TimeLog";

    /// <summary>
    /// Gets the system name of the "Staff" customer role
    /// </summary>
    public const string StaffRoleSystemName = "TimeLogStaff";

    /// <summary>
    /// Gets the system name of the "Manager" customer role
    /// </summary>
    public const string ManagerRoleSystemName = "TimeLogManager";

    /// <summary>
    /// Gets the system name of the "Project Manager" customer role (Phase 2 - governs project
    /// setup/staff-assignment access, distinct from the "Manager" role above which governs
    /// cross-staff Time Log oversight - a customer may hold either, both, or neither)
    /// </summary>
    public const string ProjectManagerRoleSystemName = "TimeLogProjectManager";

    /// <summary>
    /// Gets the generic attribute key used to flag that the Manager role was created by this plugin
    /// (used on uninstall to decide whether the role may safely be removed)
    /// </summary>
    public const string ManagerRoleCreatedByPluginAttribute = "TimeLogManagerRoleCreatedByPlugin";

    /// <summary>
    /// Gets the generic attribute key used to flag that the Staff role was created by this plugin
    /// (used on uninstall to decide whether the role may safely be removed - core nopCommerce 5.00 does
    /// not ship a built-in "Staff" role, so it needs the same conditional find-or-create/delete treatment
    /// as the Manager role - see TimeLogPlugin's remarks)
    /// </summary>
    public const string StaffRoleCreatedByPluginAttribute = "TimeLogStaffRoleCreatedByPlugin";

    /// <summary>
    /// Gets the generic attribute key used to flag that the Project Manager role was created by this
    /// plugin (used on uninstall to decide whether the role may safely be removed - same conditional
    /// find-or-create/delete treatment as the Staff/Manager roles)
    /// </summary>
    public const string ProjectManagerRoleCreatedByPluginAttribute = "TimeLogProjectManagerRoleCreatedByPlugin";

    /// <summary>
    /// Gets the maximum page size allowed for any Time Log list query (server-side clamp)
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Gets the system name of the "Time Log" top-level admin menu section
    /// </summary>
    public const string TimeLogAdminMenuSystemName = "Nop.Plugin.Misc.TimeLog.TimeLogAdminMenu";

    /// <summary>
    /// Gets the system name of the "Log Time" admin menu item (own time entries)
    /// </summary>
    public const string LogTimeAdminMenuSystemName = "Nop.Plugin.Misc.TimeLog.LogTimeAdminMenu";

    /// <summary>
    /// Gets the system name of the "Time Logs (All Staff)" admin menu item (manager oversight)
    /// </summary>
    public const string TimeLogAllAdminMenuSystemName = "Nop.Plugin.Misc.TimeLog.TimeLogAllAdminMenu";

    /// <summary>
    /// Gets the system name of the "Project" admin menu item (Phase 2 - project setup/staff assignment)
    /// </summary>
    public const string ProjectAdminMenuSystemName = "Nop.Plugin.Misc.TimeLog.ProjectAdminMenu";

    /// <summary>
    /// Gets the cache key pattern for the active projects list
    /// </summary>
    /// <remarks>
    /// No format parameters - the active project list is not store/customer scoped
    /// </remarks>
    public static CacheKey ActiveProjectsCacheKey => new("Nop.plugins.misc.timelog.project.active");

    /// <summary>
    /// Gets the key prefix used to clear cached project data on insert/update
    /// </summary>
    public static string ProjectsPatternCacheKey => "Nop.plugins.misc.timelog.project.";

    /// <summary>
    /// Gets the single shared set of <see cref="ProjectStatus"/> values that are eligible for new
    /// time-log entries (Story P2-11/P2-12). Used both when building the staff dropdown/filter
    /// (<see cref="Services.IProjectService.GetProjectsAssignedToCustomerAsync"/>) and when
    /// re-validating a posted ProjectId server-side, so the two checks can never diverge.
    /// </summary>
    public static HashSet<ProjectStatus> TimeLoggableProjectStatuses { get; } = new()
    {
        ProjectStatus.NotStarted,
        ProjectStatus.Inprogress
    };
}
