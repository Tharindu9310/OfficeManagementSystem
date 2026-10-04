using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Expenses.Models.Binding;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Expenses.Models.Admin;

/// <summary>
/// Represents an expense category row model. Thin view model; never binds to the domain entity
/// </summary>
public record ExpenseCategoryModel : BaseNopEntityModel
{
    #region Properties

    /// <summary>
    /// Gets or sets the name (stored, default-language value)
    /// </summary>
    [NopResourceDisplayName("Admin.Expenses.ExpenseCategory.Fields.Name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the raw per month limit (null = no limit)
    /// </summary>
    [NopResourceDisplayName("Admin.Expenses.ExpenseCategory.Fields.PerMonthLimit")]
    [ModelBinder(typeof(StrictNullableDecimalModelBinder))]
    public decimal? PerMonthLimit { get; set; }

    /// <summary>
    /// Gets or sets the per month limit formatted in the primary store currency (empty when there is no limit)
    /// </summary>
    [NopResourceDisplayName("Admin.Expenses.ExpenseCategory.Fields.PerMonthLimit")]
    public string PerMonthLimitDisplay { get; set; }

    /// <summary>
    /// Gets or sets the category type identifier
    /// </summary>
    [NopResourceDisplayName("Admin.Expenses.ExpenseCategory.Fields.Type")]
    public int ExpenseCategoryTypeId { get; set; }

    /// <summary>
    /// Gets or sets the localized category type text
    /// </summary>
    [NopResourceDisplayName("Admin.Expenses.ExpenseCategory.Fields.Type")]
    public string ExpenseCategoryTypeName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the category is limited to selected stores (display flag)
    /// </summary>
    [NopResourceDisplayName("Admin.Expenses.ExpenseCategory.Fields.LimitedToStores")]
    public bool LimitedToStores { get; set; }

    #endregion
}
