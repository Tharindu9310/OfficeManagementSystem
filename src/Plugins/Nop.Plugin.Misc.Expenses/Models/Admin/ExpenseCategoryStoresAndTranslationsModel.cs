using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Expenses.Models.Admin;

/// <summary>
/// Represents the "Edit stores and translations" modal model. It deliberately carries no per month limit
/// or type, so a save from the modal can never change them
/// </summary>
public record ExpenseCategoryStoresAndTranslationsModel : BaseNopEntityModel, IStoreMappingSupportedModel,
    ILocalizedModel<ExpenseCategoryLocalizedModel>
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpenseCategoryStoresAndTranslationsModel"/> class
    /// </summary>
    public ExpenseCategoryStoresAndTranslationsModel()
    {
        Locales = new List<ExpenseCategoryLocalizedModel>();
        SelectedStoreIds = new List<int>();
        AvailableStores = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the stored default-language name (read-only reference; never read on save)
    /// </summary>
    [NopResourceDisplayName("Admin.Expenses.ExpenseCategory.Fields.Name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the identifiers of the stores the category is available in (empty = all stores)
    /// </summary>
    [NopResourceDisplayName("Admin.Expenses.ExpenseCategory.Fields.SelectedStoreIds")]
    public IList<int> SelectedStoreIds { get; set; }

    /// <summary>
    /// Gets or sets the stores to choose from
    /// </summary>
    public IList<SelectListItem> AvailableStores { get; set; }

    /// <summary>
    /// Gets or sets the per-language translated names
    /// </summary>
    public IList<ExpenseCategoryLocalizedModel> Locales { get; set; }

    #endregion
}
