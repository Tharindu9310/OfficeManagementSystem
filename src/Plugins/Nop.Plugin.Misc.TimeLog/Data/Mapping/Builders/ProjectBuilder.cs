using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TimeLog.Domain;

namespace Nop.Plugin.Misc.TimeLog.Data.Mapping.Builders;

/// <summary>
/// Represents a <see cref="Project"/> entity builder
/// </summary>
/// <remarks>
/// Introduced in Phase 2 so a fresh install picks up an explicit mapping for the
/// <see cref="Project.Description"/> column (unbounded text). The Phase 1
/// <c>SchemaMigration</c> created the table via <c>Create.TableFor&lt;Project&gt;()</c> with no
/// explicit builder (default column types) and is not modified retroactively - this builder is
/// additive and only affects columns that did not already exist at that point.
/// </remarks>
public class ProjectBuilder : NopEntityBuilder<Project>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(Project.Description)).AsString(int.MaxValue).Nullable();
    }

    #endregion
}
