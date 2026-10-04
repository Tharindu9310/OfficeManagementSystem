using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Expenses.Controllers;
using Nop.Plugin.Misc.Expenses.Infrastructure;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;
using Xunit;

namespace Nop.Plugin.Misc.Expenses.Tests.Infrastructure;

/// <summary>
/// Reflection tests proving every endpoint of <see cref="ExpenseCategoryController"/> is permission-gated
/// (TT-027; AC2.1, AC2.3, AC5.10). A public action added later without the attribute fails these tests.
/// </summary>
public class PermissionCoverageTests
{
    private static readonly string[] ExpectedActions =
    {
        nameof(ExpenseCategoryController.List),
        nameof(ExpenseCategoryController.ExpenseCategoryList),
        nameof(ExpenseCategoryController.ExpenseCategoryInsert),
        nameof(ExpenseCategoryController.ExpenseCategoryUpdate),
        nameof(ExpenseCategoryController.EditStoresAndTranslations),
        nameof(ExpenseCategoryController.SaveStoresAndTranslations)
    };

    /// <summary>
    /// Every public instance method declared on the controller is an action (no [NonAction] is used on it)
    /// </summary>
    private static IEnumerable<MethodInfo> GetActions() => typeof(ExpenseCategoryController)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(method => !method.IsSpecialName && method.GetCustomAttribute<NonActionAttribute>() == null);

    [Fact]
    public void Controller_HasExactlyTheSixDocumentedActions()
    {
        Assert.Equal(ExpectedActions.OrderBy(name => name), GetActions().Select(method => method.Name).OrderBy(name => name));
    }

    [Fact]
    public void Controller_HasClassLevelAdminAreaAuthorizeAndAntiforgery()
    {
        var type = typeof(ExpenseCategoryController);

        Assert.Equal(AreaNames.ADMIN, type.GetCustomAttribute<AreaAttribute>()?.RouteValue);
        Assert.NotNull(type.GetCustomAttribute<AuthorizeAdminAttribute>());
        Assert.NotNull(type.GetCustomAttribute<AutoValidateAntiforgeryTokenAttribute>());
    }

    [Theory]
    [InlineData(nameof(ExpenseCategoryController.List))]
    [InlineData(nameof(ExpenseCategoryController.ExpenseCategoryList))]
    [InlineData(nameof(ExpenseCategoryController.ExpenseCategoryInsert))]
    [InlineData(nameof(ExpenseCategoryController.ExpenseCategoryUpdate))]
    [InlineData(nameof(ExpenseCategoryController.EditStoresAndTranslations))]
    [InlineData(nameof(ExpenseCategoryController.SaveStoresAndTranslations))]
    public void Action_IsGatedByTheManagePermission(string actionName)
    {
        var method = Assert.Single(GetActions(), m => m.Name == actionName);

        var attribute = Assert.Single(method.GetCustomAttributes<CheckPermissionAttribute>());
        Assert.Equal(new[] { PermissionProvider.ManageExpenseCategories }, attribute.PermissionSystemName);
    }

    [Fact]
    public void NoPublicAction_IsMissingTheManagePermission()
    {
        var unprotected = GetActions()
            .Where(method => !method.GetCustomAttributes<CheckPermissionAttribute>()
                .Any(attribute => attribute.PermissionSystemName.Contains(PermissionProvider.ManageExpenseCategories)))
            .Select(method => method.Name)
            .ToList();

        Assert.Empty(unprotected);
    }

    [Fact]
    public void Controller_HasNoDeleteAction()
    {
        Assert.DoesNotContain(GetActions(), method => method.Name.Contains("Delete", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void WriteActions_ArePostOnly()
    {
        foreach (var name in new[] { nameof(ExpenseCategoryController.ExpenseCategoryInsert),
                     nameof(ExpenseCategoryController.ExpenseCategoryUpdate),
                     nameof(ExpenseCategoryController.SaveStoresAndTranslations),
                     nameof(ExpenseCategoryController.ExpenseCategoryList) })
        {
            var method = Assert.Single(GetActions(), m => m.Name == name);
            Assert.NotNull(method.GetCustomAttribute<HttpPostAttribute>());
        }
    }

    [Fact]
    public void PermissionProvider_HasOnePermissionMappedToAdministrators()
    {
        var config = Assert.Single(new PermissionProvider().AllConfigs);

        Assert.Equal(PermissionProvider.ManageExpenseCategories, config.SystemName);
        Assert.Equal(new[] { NopCustomerDefaults.AdministratorsRoleName }, config.DefaultCustomerRoles);
    }
}
