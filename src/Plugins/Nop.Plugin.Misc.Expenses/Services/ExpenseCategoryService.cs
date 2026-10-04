using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Misc.Expenses.Domain;

namespace Nop.Plugin.Misc.Expenses.Services;

/// <summary>
/// Represents an expense category service
/// </summary>
public class ExpenseCategoryService : IExpenseCategoryService
{
    #region Fields

    protected readonly IRepository<ExpenseCategory> _expenseCategoryRepository;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IRepository<StoreMapping> _storeMappingRepository;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpenseCategoryService"/> class
    /// </summary>
    /// <param name="expenseCategoryRepository">Expense category repository</param>
    /// <param name="staticCacheManager">Static cache manager</param>
    /// <param name="storeMappingRepository">Store mapping repository</param>
    public ExpenseCategoryService(IRepository<ExpenseCategory> expenseCategoryRepository,
        IStaticCacheManager staticCacheManager,
        IRepository<StoreMapping> storeMappingRepository)
    {
        _expenseCategoryRepository = expenseCategoryRepository;
        _staticCacheManager = staticCacheManager;
        _storeMappingRepository = storeMappingRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets all expense categories, paged, optionally filtered by name and store
    /// </summary>
    /// <param name="name">Name to search for (trimmed, case-insensitive "contains" on the stored name); empty lists all</param>
    /// <param name="storeId">
    /// Store identifier; when greater than 0, lists categories that are not limited to stores plus those mapped to
    /// this store, regardless of <c>CatalogSettings.IgnoreStoreLimitations</c>; 0 lists all categories
    /// </param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size (clamped to <see cref="NopExpensesDefaults.MaxPageSize"/>)</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the paged list of expense categories, ordered by name then identifier
    /// </returns>
    public virtual async Task<IPagedList<ExpenseCategory>> GetAllExpenseCategoriesAsync(string name = null,
        int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        pageSize = Math.Clamp(pageSize, 1, NopExpensesDefaults.MaxPageSize);
        pageIndex = Math.Max(pageIndex, 0);

        var query = _expenseCategoryRepository.Table;

        var term = name?.Trim();

        if (!string.IsNullOrEmpty(term))
        {
            term = term.ToLowerInvariant();
            query = query.Where(c => c.Name.ToLower().Contains(term));
        }

        if (storeId > 0)
        {
            //own store-mapping filter: the stock ApplyStoreMapping returns the query unfiltered when
            //CatalogSettings.IgnoreStoreLimitations is on (or no mapping rows exist), so it is not used here (BUG-010);
            //one parameterized sub-select, executed together with the paged query
            var entityName = nameof(ExpenseCategory);
            var mappedIds = _storeMappingRepository.Table
                .Where(sm => sm.EntityName == entityName && sm.StoreId == storeId)
                .Select(sm => sm.EntityId);

            query = query.Where(c => !c.LimitedToStores || mappedIds.Contains(c.Id));
        }

        query = query.OrderBy(c => c.Name).ThenBy(c => c.Id);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets an expense category by identifier
    /// </summary>
    /// <param name="expenseCategoryId">Expense category identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the expense category, or null when not found
    /// </returns>
    public virtual async Task<ExpenseCategory> GetExpenseCategoryByIdAsync(int expenseCategoryId)
    {
        return await _expenseCategoryRepository.GetByIdAsync(expenseCategoryId);
    }

    /// <summary>
    /// Inserts an expense category (sets both timestamps)
    /// </summary>
    /// <param name="expenseCategory">Expense category</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertExpenseCategoryAsync(ExpenseCategory expenseCategory)
    {
        ArgumentNullException.ThrowIfNull(expenseCategory);

        var now = DateTime.UtcNow;
        expenseCategory.CreatedOnUtc = now;
        expenseCategory.UpdatedOnUtc = now;

        await _expenseCategoryRepository.InsertAsync(expenseCategory);
        await _staticCacheManager.RemoveByPrefixAsync(NopExpensesDefaults.ExpenseCategoriesPatternCacheKey);
    }

    /// <summary>
    /// Updates an expense category (sets the updated timestamp)
    /// </summary>
    /// <param name="expenseCategory">Expense category</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateExpenseCategoryAsync(ExpenseCategory expenseCategory)
    {
        ArgumentNullException.ThrowIfNull(expenseCategory);

        expenseCategory.UpdatedOnUtc = DateTime.UtcNow;

        await _expenseCategoryRepository.UpdateAsync(expenseCategory);
        await _staticCacheManager.RemoveByPrefixAsync(NopExpensesDefaults.ExpenseCategoriesPatternCacheKey);
    }

    #endregion
}
