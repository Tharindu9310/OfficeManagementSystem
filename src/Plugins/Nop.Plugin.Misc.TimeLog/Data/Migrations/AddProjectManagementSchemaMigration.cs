using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TimeLog.Domain;

namespace Nop.Plugin.Misc.TimeLog.Data.Migrations;

/// <summary>
/// Represents the Phase 2 (Project Management) schema migration - alters the existing
/// <see cref="Project"/> table and creates the new <see cref="ProjectStaffMapping"/> table.
/// </summary>
/// <remarks>
/// This is a separate, <see cref="MigrationProcessType.Update"/> migration - the Phase 1
/// <c>SchemaMigration</c> (<see cref="MigrationProcessType.Installation"/>) is not edited
/// retroactively, since doing so would break stores that already installed Phase 1.
/// </remarks>
[NopMigration("2026/09/15 00:00:00", "Nop.Plugin.Misc.TimeLog Phase 2 schema", MigrationProcessType.Update)]
public class AddProjectManagementSchemaMigration : AutoReversingMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        //extend the existing Project table with the new Phase 2 columns
        Alter.Table(NameCompatibilityManager.GetTableName(typeof(Project)))
            .AddColumn(nameof(Project.StartDate)).AsDateTime2().NotNullable().WithDefaultValue(DateTime.UtcNow)
            .AddColumn(nameof(Project.EndDate)).AsDateTime2().Nullable()
            .AddColumn(nameof(Project.Description)).AsString(int.MaxValue).Nullable()
            .AddColumn(nameof(Project.StatusId)).AsInt32().NotNullable().WithDefaultValue(0)
            .AddColumn(nameof(Project.CreatedOnUtc)).AsDateTime2().NotNullable().WithDefaultValue(DateTime.UtcNow)
            .AddColumn(nameof(Project.UpdatedOnUtc)).AsDateTime2().NotNullable().WithDefaultValue(DateTime.UtcNow)
            .AddColumn(nameof(Project.LimitedToStores)).AsBoolean().NotNullable().WithDefaultValue(false);

        //create the new staff assignment mapping table
        Create.TableFor<ProjectStaffMapping>();
    }
}
