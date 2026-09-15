using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.TimeLog.Domain;

namespace Nop.Plugin.Misc.TimeLog.Data.Migrations;

/// <summary>
/// Represents the initial schema migration for the Time Log plugin
/// </summary>
[NopMigration("2025/01/01 00:00:00", "Nop.Plugin.Misc.TimeLog schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        Create.TableFor<Project>();
        Create.TableFor<Domain.TimeLog>();
    }
}
