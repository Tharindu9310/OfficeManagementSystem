using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TimeLog.Domain;

namespace Nop.Plugin.Misc.TimeLog.Data.Mapping.Builders;

/// <summary>
/// Represents a <see cref="ProjectStaffMapping"/> entity builder
/// </summary>
public class ProjectStaffMappingBuilder : NopEntityBuilder<ProjectStaffMapping>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(ProjectStaffMapping.ProjectId)).AsInt32().ForeignKey<Project>();
        table.WithColumn(nameof(ProjectStaffMapping.CustomerId)).AsInt32().ForeignKey<Customer>();
    }

    #endregion
}
