using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.Expenses.Domain;

namespace Nop.Plugin.Misc.Expenses.Data.Mapping.Builders;

/// <summary>
/// Represents an <see cref="ExpenseCategory"/> entity builder
/// </summary>
/// <remarks>
/// The table keeps the default name <c>ExpenseCategory</c> (no INameCompatibility). Plugin name
/// compatibilities are only loaded for plugins already marked installed, which is not yet true while the
/// install migration runs, so the table could be created and later mapped under different names.
/// <c>ExpenseCategory</c> does not collide with any core table. There is deliberately no unique index on
/// Name (duplicate names are allowed).
/// </remarks>
public class ExpenseCategoryBuilder : NopEntityBuilder<ExpenseCategory>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ExpenseCategory.Name)).AsString(200).NotNullable()
            .WithColumn(nameof(ExpenseCategory.PerMonthLimit)).AsDecimal(18, 4).Nullable()
            .WithColumn(nameof(ExpenseCategory.ExpenseCategoryTypeId)).AsInt32().NotNullable()
            .WithColumn(nameof(ExpenseCategory.LimitedToStores)).AsBoolean().NotNullable().WithDefaultValue(false);
    }

    #endregion
}
