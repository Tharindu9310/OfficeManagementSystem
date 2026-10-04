using Xunit;
using Nop.Plugin.Misc.Expenses.Infrastructure;

namespace Nop.Plugin.Misc.Expenses.Tests.Infrastructure;

/// <summary>
/// Tests for the locale resource inventory (TT-010)
/// </summary>
public class ExpensesLocaleResourceTests
{
    private static readonly string[] AllowedPrefixes =
    {
        "Admin.Expenses.",
        "Enums.Nop.Plugin.Misc.Expenses.Domain.ExpenseCategoryType.",
        "Security.Permission.Misc.Expenses."
    };

    [Fact]
    public void GetLocaleResources_ContainsMenuAndPermissionKeys()
    {
        var keys = ExpensesPlugin.GetLocaleResources().Keys;

        Assert.Contains("Admin.Expenses.Menu.Expenses", keys);
        Assert.Contains("Admin.Expenses.Menu.ExpenseCategory", keys);
        Assert.Contains($"Security.Permission.{PermissionProvider.ManageExpenseCategories}", keys);
    }

    [Fact]
    public void GetLocaleResources_HasNoDuplicateNameKey()
    {
        Assert.DoesNotContain(ExpensesPlugin.GetLocaleResources().Keys, k => k.Contains("NameDuplicate"));
    }

    [Fact]
    public void GetLocaleResources_AllKeysFallUnderPrefixesTheUninstallRemoves()
    {
        foreach (var key in ExpensesPlugin.GetLocaleResources().Keys)
            Assert.Contains(AllowedPrefixes, prefix => key.StartsWith(prefix, StringComparison.Ordinal));
    }

    [Fact]
    public void GetLocaleResources_AllValuesAreNotEmpty()
    {
        Assert.All(ExpensesPlugin.GetLocaleResources(), pair => Assert.False(string.IsNullOrWhiteSpace(pair.Value), pair.Key));
    }

    [Theory]
    [InlineData("Admin.Expenses.ExpenseCategory.Validation.NameTooLong")]
    [InlineData("Admin.Expenses.ExpenseCategory.Validation.PerMonthLimitTooLarge")]
    [InlineData("Admin.Expenses.ExpenseCategory.Validation.PerMonthLimitTooManyDecimals")]
    public void GetLocaleResources_LimitMessages_UsePlaceholdersInsteadOfHardcodedNumbers(string key)
    {
        var text = ExpensesPlugin.GetLocaleResources()[key];

        Assert.Contains("{0}", text);
        Assert.DoesNotContain("200", text);
    }

    [Theory]
    [InlineData("Admin.Expenses.ExpenseCategory.Validation.InvalidRequest")]
    [InlineData("Admin.Expenses.ExpenseCategory.Error.Generic")]
    [InlineData("Admin.Expenses.ExpenseCategory.StoresAndTranslations.StoresHint")]
    public void GetLocaleResources_ContainsTheKeysAddedByTheBugFixes(string key)
    {
        Assert.Contains(key, ExpensesPlugin.GetLocaleResources().Keys);
    }

    [Fact]
    public void GetLocaleResources_DoesNotShipTheUnusedAccessibilityRowKeys()
    {
        var keys = ExpensesPlugin.GetLocaleResources().Keys;

        Assert.DoesNotContain(keys, k => k.EndsWith("Accessibility.EditRow") || k.EndsWith("Accessibility.SaveRow") || k.EndsWith("Accessibility.CancelEdit"));
    }
}
