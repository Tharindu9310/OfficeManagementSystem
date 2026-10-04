namespace Nop.Plugin.Misc.Expenses;

/// <summary>
/// Represents plugin constants
/// </summary>
public static class NopExpensesDefaults
{
    /// <summary>
    /// Gets the plugin system name (matches plugin.json's "SystemName")
    /// </summary>
    public const string SystemName = "Misc.Expenses";

    /// <summary>
    /// Gets the maximum page size allowed for any list endpoint (enforced server-side)
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Gets the maximum length of an expense category name (also applied to each translated name)
    /// </summary>
    public const int NameMaxLength = 200;

    /// <summary>
    /// Gets the largest per month limit that fits the decimal(18,4) column (14 integer digits)
    /// </summary>
    public const decimal PerMonthLimitMaxValue = 99999999999999.9999m;

    /// <summary>
    /// Gets the number of decimal places the decimal(18,4) column stores
    /// </summary>
    public const int PerMonthLimitMaxScale = 4;

    /// <summary>
    /// Gets the prefix of the per-customer generic attribute keys the Expense Category page stores
    /// (collapsed state of its panels, saved by the core Preferences/SavePreference action)
    /// </summary>
    public const string PageAttributePrefix = "ExpenseCategoryPage.";

    /// <summary>
    /// Gets the generic attribute key remembering that the search panel is collapsed
    /// </summary>
    public const string HideSearchBlockAttributeName = PageAttributePrefix + "HideSearchBlock";

    /// <summary>
    /// Gets the generic attribute key remembering that the Add new panel is collapsed
    /// </summary>
    public const string HideAddPanelAttributeName = PageAttributePrefix + "HideAddPanel";

    /// <summary>
    /// Gets the system name of the top-level "Expenses" admin menu item
    /// </summary>
    public const string ExpensesAdminMenuSystemName = "Nop.Plugin.Misc.Expenses.ExpensesAdminMenu";

    /// <summary>
    /// Gets the system name of the "Expense Category" admin menu item
    /// </summary>
    public const string ExpenseCategoryAdminMenuSystemName = "Nop.Plugin.Misc.Expenses.ExpenseCategoryAdminMenu";

    /// <summary>
    /// Gets the key prefix used to clear cached expense category data on insert/update
    /// </summary>
    public static string ExpenseCategoriesPatternCacheKey => "Nop.plugins.misc.expenses.expensecategory.";
}
