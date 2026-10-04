using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;

namespace Nop.Plugin.Misc.Expenses.Domain;

/// <summary>
/// Represents an expense (or income) category
/// </summary>
public class ExpenseCategory : BaseEntity, ILocalizedEntity, IStoreMappingSupported
{
    /// <summary>
    /// Gets or sets the category name (the default-language value; other languages are stored as
    /// localized properties)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional monthly limit in the primary store currency; null means no limit.
    /// Informational only
    /// </summary>
    public decimal? PerMonthLimit { get; set; }

    /// <summary>
    /// Gets or sets the category type identifier (backing field for <see cref="ExpenseCategoryType"/>)
    /// </summary>
    public int ExpenseCategoryTypeId { get; set; }

    /// <summary>
    /// Gets or sets the category type
    /// </summary>
    [NotMapped]
    public ExpenseCategoryType ExpenseCategoryType
    {
        get => (ExpenseCategoryType)ExpenseCategoryTypeId;
        set => ExpenseCategoryTypeId = (int)value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the category is limited to certain stores
    /// </summary>
    public bool LimitedToStores { get; set; }

    /// <summary>
    /// Gets or sets the date and time (UTC) the category was created
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time (UTC) the category was last updated
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }
}
