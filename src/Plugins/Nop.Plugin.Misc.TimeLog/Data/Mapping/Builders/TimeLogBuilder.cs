using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TimeLog.Domain;

namespace Nop.Plugin.Misc.TimeLog.Data.Mapping.Builders;

/// <summary>
/// Represents a <see cref="Domain.TimeLog"/> entity builder
/// </summary>
public class TimeLogBuilder : NopEntityBuilder<Domain.TimeLog>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(Domain.TimeLog.ProjectId)).AsInt32().ForeignKey<Project>();
        // decimal(9,6): resolved precision for TT-003 (supersedes an earlier decimal(5,4) draft) -
        // do not narrow this back down, see comment on Domain.TimeLog.Time.
        table.WithColumn(nameof(Domain.TimeLog.Time)).AsDecimal(9, 6);
    }

    #endregion
}
