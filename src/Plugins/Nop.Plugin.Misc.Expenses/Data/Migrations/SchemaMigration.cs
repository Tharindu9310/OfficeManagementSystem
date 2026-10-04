using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Expenses.Domain;

namespace Nop.Plugin.Misc.Expenses.Data.Migrations;

/// <summary>
/// Represents the initial schema migration for the Expenses plugin. The timestamp literal must stay
/// unique and unchanged after release.
/// </summary>
[NopMigration("2026/10/04 00:00:00", "Nop.Plugin.Misc.Expenses schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        Create.TableFor<ExpenseCategory>();
    }
}
