namespace Nop.Plugin.Misc.Expenses.Domain;

/// <summary>
/// Represents the type of an expense category. Values start at 1 so that an unset value (0) is invalid.
/// </summary>
public enum ExpenseCategoryType
{
    /// <summary>
    /// Money spent
    /// </summary>
    Expense = 1,

    /// <summary>
    /// Money received
    /// </summary>
    Income = 2
}
