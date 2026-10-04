using Nop.Core;
using Nop.Plugin.Misc.Expenses.Domain;

namespace Nop.Plugin.Misc.Expenses.Services;

/// <summary>
/// Represents an expense category service
/// </summary>
public interface IExpenseCategoryService
{
    /// <summary>
    /// Gets all expense categories, paged, optionally filtered by name and store
    /// </summary>
    /// <param name="name">Name to search for (trimmed, case-insensitive "contains" on the stored name); empty lists all</param>
    /// <param name="storeId">Store identifier for store-mapping filtering; 0 to list all categories</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size (clamped to <see cref="NopExpensesDefaults.MaxPageSize"/>)</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the paged list of expense categories, ordered by name then identifier
    /// </returns>
    Task<IPagedList<ExpenseCategory>> GetAllExpenseCategoriesAsync(string name = null, int storeId = 0,
        int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// Gets an expense category by identifier
    /// </summary>
    /// <param name="expenseCategoryId">Expense category identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the expense category, or null when not found
    /// </returns>
    Task<ExpenseCategory> GetExpenseCategoryByIdAsync(int expenseCategoryId);

    /// <summary>
    /// Inserts an expense category (sets both timestamps)
    /// </summary>
    /// <param name="expenseCategory">Expense category</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertExpenseCategoryAsync(ExpenseCategory expenseCategory);

    /// <summary>
    /// Updates an expense category (sets the updated timestamp)
    /// </summary>
    /// <param name="expenseCategory">Expense category</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateExpenseCategoryAsync(ExpenseCategory expenseCategory);
}
