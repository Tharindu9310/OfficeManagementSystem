using Xunit;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Expenses.Infrastructure;

namespace Nop.Plugin.Misc.Expenses.Tests.Infrastructure;

/// <summary>
/// Tests for <see cref="PermissionProvider"/> (TT-009)
/// </summary>
public class PermissionProviderTests
{
    [Fact]
    public void AllConfigs_HasExactlyOnePermission_GrantedToAdministrators()
    {
        var configs = new PermissionProvider().AllConfigs;

        var config = Assert.Single(configs);
        Assert.Equal(PermissionProvider.ManageExpenseCategories, config.SystemName);
        Assert.Equal(PermissionProvider.Category, config.Category);
        Assert.Equal(new[] { NopCustomerDefaults.AdministratorsRoleName }, config.DefaultCustomerRoles);
    }
}
