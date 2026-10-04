using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.Expenses.Infrastructure;

/// <summary>
/// Represents the Expenses permission provider.
/// </summary>
/// <remarks>
/// nopCommerce discovers all <see cref="IPermissionConfigManager"/> implementations via <c>ITypeFinder</c>
/// and instantiates them with <c>Activator.CreateInstance</c>, so this class is not registered in DI and
/// must expose a public parameterless constructor. The permission record and its Administrators mapping
/// are inserted by the framework on the next application start after the plugin is installed.
/// </remarks>
public class PermissionProvider : IPermissionConfigManager
{
    #region Permission system names

    /// <summary>
    /// Gets the system name of the permission that governs the Expenses menu and every Expense Category
    /// endpoint (grid read, add, update, stores and translations modal)
    /// </summary>
    public const string ManageExpenseCategories = "Misc.Expenses.ManageExpenseCategories";

    /// <summary>
    /// Gets the permission category used for all Expenses permission records
    /// </summary>
    public const string Category = "Misc.Expenses";

    #endregion

    #region Methods

    /// <summary>
    /// Gets all permission configurations (one permission, granted to the Administrators role by default)
    /// </summary>
    public IList<PermissionConfig> AllConfigs =>
        new List<PermissionConfig>
        {
            new("Admin area. Manage expense categories", ManageExpenseCategories, Category, NopCustomerDefaults.AdministratorsRoleName)
        };

    #endregion
}
