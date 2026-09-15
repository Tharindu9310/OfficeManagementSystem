using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.TimeLog.Infrastructure;

/// <summary>
/// Represents the Time Log permission provider.
/// </summary>
/// <remarks>
/// Follows the same pattern as Nop.Plugin.Misc.RFQ.Services.RfqPermissionConfigManager:
/// nopCommerce 5.00 discovers all <see cref="IPermissionConfigManager"/> implementations
/// via <c>ITypeFinder</c> (see Nop.Services.Security.PermissionService.InstallPermissionsAsync),
/// so this class does not need to be registered manually in DI, and it is instantiated with
/// <c>Activator.CreateInstance</c> - it must expose a public parameterless constructor.
/// </remarks>
public class PermissionProvider : IPermissionConfigManager
{
    #region Permission system names

    /// <summary>
    /// Gets the system name of the permission that allows staff to log and manage their own time entries
    /// </summary>
    public const string ManageTimeLog = "Misc.TimeLog.ManageTimeLog";

    /// <summary>
    /// Gets the system name of the permission that allows managers to view all staff time entries
    /// </summary>
    public const string ManageTimeLogAll = "Misc.TimeLog.ManageTimeLogAll";

    /// <summary>
    /// Gets the system name of the permission that allows Project Managers to manage projects and
    /// staff assignments (Phase 2). Kept separate from <see cref="ManageTimeLogAll"/> - the two
    /// concerns (time-log oversight vs. project setup/staffing) are independent; a customer may
    /// hold either, both, or neither.
    /// </summary>
    public const string ManageProjects = "Misc.TimeLog.ManageProjects";

    /// <summary>
    /// Gets the permission category used for all Time Log permission records
    /// </summary>
    public const string Category = "Misc.TimeLog";

    #endregion

    #region Methods

    /// <summary>
    /// Gets all permission configurations
    /// </summary>
    public IList<PermissionConfig> AllConfigs =>
        new List<PermissionConfig>
        {
            new("Admin area. Log and manage own time log entries", ManageTimeLog, Category, NopTimeLogDefaults.StaffRoleSystemName),
            new("Admin area. View time log entries for all staff", ManageTimeLogAll, Category, NopTimeLogDefaults.ManagerRoleSystemName),
            new("Admin area. Manage projects and staff assignments", ManageProjects, Category, NopTimeLogDefaults.ProjectManagerRoleSystemName)
        };

    /// <summary>
    /// Finds the "Staff" customer role by its system name, creating it if it does not already exist
    /// </summary>
    /// <param name="customerService">Customer service</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the Staff <see cref="CustomerRole"/> and a flag indicating whether it was newly created
    /// </returns>
    public static async Task<(CustomerRole Role, bool Created)> GetOrCreateStaffRoleAsync(ICustomerService customerService)
    {
        ArgumentNullException.ThrowIfNull(customerService);

        var role = await customerService.GetCustomerRoleBySystemNameAsync(NopTimeLogDefaults.StaffRoleSystemName);

        if (role is not null)
            return (role, false);

        role = new CustomerRole
        {
            Name = "Staff",
            Active = true,
            SystemName = NopTimeLogDefaults.StaffRoleSystemName
        };

        await customerService.InsertCustomerRoleAsync(role);

        return (role, true);
    }

    /// <summary>
    /// Finds the "Manager" customer role by its system name, creating it if it does not already exist
    /// </summary>
    /// <param name="customerService">Customer service</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the Manager <see cref="CustomerRole"/> and a flag indicating whether it was newly created
    /// </returns>
    /// <remarks>
    /// TT-016's InstallAsync must record <see cref="NopTimeLogDefaults.ManagerRoleCreatedByPluginAttribute"/> as a
    /// generic attribute on the returned role only when <c>Created</c> is true, so that UninstallAsync can later
    /// decide whether it is safe to remove the role.
    /// </remarks>
    public static async Task<(CustomerRole Role, bool Created)> GetOrCreateManagerRoleAsync(ICustomerService customerService)
    {
        ArgumentNullException.ThrowIfNull(customerService);

        var role = await customerService.GetCustomerRoleBySystemNameAsync(NopTimeLogDefaults.ManagerRoleSystemName);

        if (role is not null)
            return (role, false);

        role = new CustomerRole
        {
            Name = "Manager",
            Active = true,
            SystemName = NopTimeLogDefaults.ManagerRoleSystemName
        };

        await customerService.InsertCustomerRoleAsync(role);

        return (role, true);
    }

    /// <summary>
    /// Finds the "Project Manager" customer role by its system name, creating it if it does not already exist
    /// </summary>
    /// <param name="customerService">Customer service</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the Project Manager <see cref="CustomerRole"/> and a flag indicating whether it was newly created
    /// </returns>
    /// <remarks>
    /// TT-030's InstallAsync must record <see cref="NopTimeLogDefaults.ProjectManagerRoleCreatedByPluginAttribute"/> as a
    /// generic attribute on the returned role only when <c>Created</c> is true, so that UninstallAsync can later
    /// decide whether it is safe to remove the role - mirrors <see cref="GetOrCreateManagerRoleAsync"/> exactly.
    /// </remarks>
    public static async Task<(CustomerRole Role, bool Created)> GetOrCreateProjectManagerRoleAsync(ICustomerService customerService)
    {
        ArgumentNullException.ThrowIfNull(customerService);

        var role = await customerService.GetCustomerRoleBySystemNameAsync(NopTimeLogDefaults.ProjectManagerRoleSystemName);

        if (role is not null)
            return (role, false);

        role = new CustomerRole
        {
            Name = "Project Manager",
            Active = true,
            SystemName = NopTimeLogDefaults.ProjectManagerRoleSystemName
        };

        await customerService.InsertCustomerRoleAsync(role);

        return (role, true);
    }

    #endregion
}
