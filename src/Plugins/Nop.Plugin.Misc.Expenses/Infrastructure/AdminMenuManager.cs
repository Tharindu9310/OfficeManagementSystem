using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.Expenses.Infrastructure;

/// <summary>
/// Registers the Expenses plugin's admin menu items (Expenses > Expense Category).
/// </summary>
/// <remarks>
/// Handled through <see cref="AdminMenuCreatedEvent"/>; consumers are discovered automatically,
/// so no DI registration is needed.
/// </remarks>
public class AdminMenuManager : IConsumer<AdminMenuCreatedEvent>
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IPluginManager<IPlugin> _pluginManager;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminMenuManager"/> class
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="pluginManager">Plugin manager</param>
    public AdminMenuManager(ILocalizationService localizationService,
        IPluginManager<IPlugin> pluginManager)
    {
        _localizationService = localizationService;
        _pluginManager = pluginManager;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handle admin menu created event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        //returns only fully installed plugins, so the menu disappears once the plugin is uninstalled
        var plugin = await _pluginManager.LoadPluginBySystemNameAsync(NopExpensesDefaults.SystemName);
        if (plugin == null)
            return;

        var expensesSection = new AdminMenuItem
        {
            Visible = true,
            SystemName = NopExpensesDefaults.ExpensesAdminMenuSystemName,
            Title = await _localizationService.GetResourceAsync("Admin.Expenses.Menu.Expenses"),
            IconClass = "fas fa-wallet",
            PermissionNames = new List<string> { PermissionProvider.ManageExpenseCategories },
            ChildNodes =
            {
                new AdminMenuItem
                {
                    Visible = true,
                    SystemName = NopExpensesDefaults.ExpenseCategoryAdminMenuSystemName,
                    Title = await _localizationService.GetResourceAsync("Admin.Expenses.Menu.ExpenseCategory"),
                    //points at Admin/ExpenseCategory/List (built by TT-013/TT-014) and returns 404 until then
                    Url = eventMessage.GetMenuItemUrl("ExpenseCategory", "List"),
                    IconClass = "far fa-dot-circle",
                    PermissionNames = new List<string> { PermissionProvider.ManageExpenseCategories }
                }
            }
        };

        //insert as a new top-level section, right before the "Third party plugins" root item
        if (!eventMessage.RootMenuItem.InsertBefore("Third party plugins", expensesSection))
            eventMessage.RootMenuItem.ChildNodes.Add(expensesSection);
    }

    #endregion
}
