using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Expenses.Models.Admin;

/// <summary>
/// Represents the expense category list model for the List grid
/// </summary>
public record ExpenseCategoryListModel : BasePagedListModel<ExpenseCategoryModel>;
