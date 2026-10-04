using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Expenses.Models.Admin;

/// <summary>
/// Represents the expense category search model (backs the List grid filters)
/// </summary>
public record ExpenseCategorySearchModel : BaseSearchModel
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpenseCategorySearchModel"/> record
    /// </summary>
    public ExpenseCategorySearchModel()
    {
        AvailableStores = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the name to search for (default-language name)
    /// </summary>
    [NopResourceDisplayName("Admin.Expenses.ExpenseCategory.Search.Name")]
    public string SearchName { get; set; }

    /// <summary>
    /// Gets or sets the store filter (0 = all stores)
    /// </summary>
    [NopResourceDisplayName("Admin.Expenses.ExpenseCategory.Search.Store")]
    public int SearchStoreId { get; set; }

    /// <summary>
    /// Gets or sets the stores available for the filter dropdown
    /// </summary>
    public IList<SelectListItem> AvailableStores { get; set; }

    #endregion
}
