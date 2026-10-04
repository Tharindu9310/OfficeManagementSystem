using System.Linq.Expressions;
using Moq;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Misc.Expenses.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Security;
using Xunit;

namespace Nop.Plugin.Misc.Expenses.Tests.Infrastructure;

/// <summary>
/// Install/Uninstall symmetry tests for <see cref="ExpensesPlugin"/> (TT-028; AC2.5, AC2.6)
/// </summary>
public class ExpensesPluginLifecycleTests
{
    private const string EntityName = "ExpenseCategory";

    private readonly Mock<ILocalizationService> _localization = new();
    private readonly Mock<IPermissionService> _permission = new();
    private readonly Mock<IRepository<LocalizedProperty>> _localizedProperties = new();
    private readonly Mock<IRepository<StoreMapping>> _storeMappings = new();
    private readonly Mock<IRepository<GenericAttribute>> _genericAttributes = new();
    private readonly List<string> _calls = new();

    private Expression<Func<StoreMapping, bool>> _storeMappingPredicate;
    private Expression<Func<LocalizedProperty, bool>> _localizedPropertyPredicate;
    private Expression<Func<GenericAttribute, bool>> _genericAttributePredicate;
    private readonly List<string> _deletedPrefixes = new();
    private readonly List<string> _deletedKeys = new();
    private IDictionary<string, string> _installed;

    private ExpensesPlugin CreatePlugin()
    {
        _localization.Setup(s => s.AddOrUpdateLocaleResourceAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<int?>()))
            .Callback<IDictionary<string, string>, int?>((resources, _) => _installed = resources)
            .Returns(Task.CompletedTask);
        _localization.Setup(s => s.DeleteLocaleResourcesAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .Callback<string, int?>((prefix, _) => { _calls.Add("prefix:" + prefix); _deletedPrefixes.Add(prefix); })
            .Returns(Task.CompletedTask);
        _localization.Setup(s => s.DeleteLocaleResourceAsync(It.IsAny<string>()))
            .Callback<string>(key => { _calls.Add("key:" + key); _deletedKeys.Add(key); })
            .Returns(Task.CompletedTask);
        _permission.Setup(s => s.DeletePermissionAsync(It.IsAny<string>()))
            .Callback<string>(name => _calls.Add("permission:" + name))
            .Returns(Task.CompletedTask);
        _storeMappings.Setup(r => r.DeleteAsync(It.IsAny<Expression<Func<StoreMapping, bool>>>()))
            .Callback<Expression<Func<StoreMapping, bool>>>(p => { _calls.Add("storemapping"); _storeMappingPredicate = p; })
            .ReturnsAsync(0);
        _localizedProperties.Setup(r => r.DeleteAsync(It.IsAny<Expression<Func<LocalizedProperty, bool>>>()))
            .Callback<Expression<Func<LocalizedProperty, bool>>>(p => { _calls.Add("localizedproperty"); _localizedPropertyPredicate = p; })
            .ReturnsAsync(0);
        _genericAttributes.Setup(r => r.DeleteAsync(It.IsAny<Expression<Func<GenericAttribute, bool>>>()))
            .Callback<Expression<Func<GenericAttribute, bool>>>(p => { _calls.Add("genericattribute"); _genericAttributePredicate = p; })
            .ReturnsAsync(0);

        return new ExpensesPlugin(_localization.Object, _permission.Object, _localizedProperties.Object, _storeMappings.Object,
            _genericAttributes.Object);
    }

    private bool IsDeleted(string key) =>
        _deletedKeys.Contains(key) || _deletedPrefixes.Any(prefix => key.StartsWith(prefix, StringComparison.Ordinal));

    [Fact]
    public async Task Install_AddsEveryLocaleResourceFromTheInventory()
    {
        await CreatePlugin().InstallAsync();

        Assert.NotNull(_installed);
        Assert.Equal(ExpensesPlugin.GetLocaleResources().OrderBy(p => p.Key), _installed.OrderBy(p => p.Key));
    }

    [Fact]
    public async Task Uninstall_DeletesThePermissionAndItsLocaleResource()
    {
        await CreatePlugin().UninstallAsync();

        _permission.Verify(s => s.DeletePermissionAsync(PermissionProvider.ManageExpenseCategories), Times.Once);
        Assert.Contains($"Security.Permission.{PermissionProvider.ManageExpenseCategories}", _deletedKeys);
    }

    [Fact]
    public async Task Uninstall_RemovesEveryInstalledLocaleKey()
    {
        await CreatePlugin().UninstallAsync();

        var leftovers = ExpensesPlugin.GetLocaleResources().Keys.Where(key => !IsDeleted(key)).ToList();

        Assert.Empty(leftovers);
    }

    [Fact]
    public async Task Uninstall_DeletesExactlyTheDeclaredPrefixes()
    {
        await CreatePlugin().UninstallAsync();

        var deleted = _deletedPrefixes.Concat(_deletedKeys).OrderBy(x => x);
        Assert.Equal(ExpensesPlugin.LocaleResourcePrefixesDeletedOnUninstall.OrderBy(x => x), deleted);
    }

    [Fact]
    public async Task Uninstall_StoreMappingDeleteTargetsOnlyExpenseCategoryRows()
    {
        await CreatePlugin().UninstallAsync();

        var matches = _storeMappingPredicate.Compile();
        Assert.True(matches(new StoreMapping { EntityName = EntityName, EntityId = 1, StoreId = 1 }));
        Assert.False(matches(new StoreMapping { EntityName = "Product", EntityId = 1, StoreId = 1 }));
        Assert.False(matches(new StoreMapping { EntityName = "Project", EntityId = 1, StoreId = 1 }));
    }

    [Fact]
    public async Task Uninstall_LocalizedPropertyDeleteTargetsOnlyExpenseCategoryRows()
    {
        await CreatePlugin().UninstallAsync();

        var matches = _localizedPropertyPredicate.Compile();
        Assert.True(matches(new LocalizedProperty { LocaleKeyGroup = EntityName, LocaleKey = "Name", EntityId = 1 }));
        Assert.False(matches(new LocalizedProperty { LocaleKeyGroup = "Product", LocaleKey = "Name", EntityId = 1 }));
        Assert.False(matches(new LocalizedProperty { LocaleKeyGroup = "Project", LocaleKey = "Name", EntityId = 1 }));
    }

    [Fact]
    public async Task Uninstall_GenericAttributeDeleteTargetsOnlyTheExpenseCategoryPagePreferences()
    {
        await CreatePlugin().UninstallAsync();

        var matches = _genericAttributePredicate.Compile();
        Assert.True(matches(new GenericAttribute { KeyGroup = nameof(Customer), Key = NopExpensesDefaults.HideSearchBlockAttributeName, EntityId = 1 }));
        Assert.True(matches(new GenericAttribute { KeyGroup = nameof(Customer), Key = NopExpensesDefaults.HideAddPanelAttributeName, EntityId = 2 }));
        //other plugins' and core page preferences, and other entities' attributes, are never touched
        Assert.False(matches(new GenericAttribute { KeyGroup = nameof(Customer), Key = "ProjectPage.HideSearchBlock", EntityId = 1 }));
        Assert.False(matches(new GenericAttribute { KeyGroup = nameof(Customer), Key = "OtherExpenseCategoryPage.HideAddPanel", EntityId = 1 }));
        Assert.False(matches(new GenericAttribute { KeyGroup = "Product", Key = NopExpensesDefaults.HideAddPanelAttributeName, EntityId = 1 }));
    }

    [Fact]
    public void PagePreferenceKeys_ShareThePrefixTheUninstallDeletes()
    {
        Assert.StartsWith(NopExpensesDefaults.PageAttributePrefix, NopExpensesDefaults.HideSearchBlockAttributeName);
        Assert.StartsWith(NopExpensesDefaults.PageAttributePrefix, NopExpensesDefaults.HideAddPanelAttributeName);
    }

    [Fact]
    public async Task Uninstall_PerformsEveryCleanupStepInTheDesignedOrder()
    {
        await CreatePlugin().UninstallAsync();

        //base.UninstallAsync() is the last statement of the method (a completed task); the table drop happens
        //in the framework after UninstallAsync returns, so everything below ran before the table goes away
        Assert.Equal(new[]
        {
            "permission:" + PermissionProvider.ManageExpenseCategories,
            "storemapping",
            "localizedproperty",
            "genericattribute",
            "prefix:Admin.Expenses",
            "prefix:Enums.Nop.Plugin.Misc.Expenses.Domain.ExpenseCategoryType",
            "key:Security.Permission." + PermissionProvider.ManageExpenseCategories
        }, _calls);
    }
}
