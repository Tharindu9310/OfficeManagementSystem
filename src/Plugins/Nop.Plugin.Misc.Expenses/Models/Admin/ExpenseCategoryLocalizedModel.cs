using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Expenses.Models.Admin;

/// <summary>
/// Represents the translated name of an expense category for one language
/// </summary>
public record ExpenseCategoryLocalizedModel : ILocalizedLocaleModel
{
    #region Properties

    /// <summary>
    /// Gets or sets the language identifier
    /// </summary>
    public int LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the language name (display only, used for the field label; never read on save)
    /// </summary>
    public string LanguageName { get; set; }

    /// <summary>
    /// Gets or sets the translated name (empty = the default name is used)
    /// </summary>
    [NopResourceDisplayName("Admin.Expenses.ExpenseCategory.Fields.Locales.Name")]
    public string Name { get; set; }

    #endregion
}
