using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Misc.Expenses.Domain;
using Nop.Plugin.Misc.Expenses.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.Expenses;

/// <summary>
/// Represents the Expenses plugin.
/// </summary>
/// <remarks>
/// The schema migration is applied by the framework before <see cref="InstallAsync"/> and reversed after
/// <see cref="UninstallAsync"/>; the permission record is inserted by the framework on the next application
/// start. Uninstall removes everything install registered (permission, locale resources) plus the core-table
/// rows the plugin created for its entity (StoreMapping, LocalizedProperty), all before the table is dropped.
/// </remarks>
public class ExpensesPlugin : BasePlugin, IMiscPlugin
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
    private readonly IRepository<StoreMapping> _storeMappingRepository;
    private readonly IRepository<GenericAttribute> _genericAttributeRepository;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpensesPlugin"/> class
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="permissionService">Permission service</param>
    /// <param name="localizedPropertyRepository">Localized property repository</param>
    /// <param name="storeMappingRepository">Store mapping repository</param>
    /// <param name="genericAttributeRepository">Generic attribute repository</param>
    public ExpensesPlugin(ILocalizationService localizationService,
        IPermissionService permissionService,
        IRepository<LocalizedProperty> localizedPropertyRepository,
        IRepository<StoreMapping> storeMappingRepository,
        IRepository<GenericAttribute> genericAttributeRepository)
    {
        _genericAttributeRepository = genericAttributeRepository;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _localizedPropertyRepository = localizedPropertyRepository;
        _storeMappingRepository = storeMappingRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Locale resource name prefixes (and the single permission key) that <see cref="UninstallAsync"/> deletes.
    /// Every key returned by <see cref="GetLocaleResources"/> must fall under one of these.
    /// </summary>
    public static IReadOnlyList<string> LocaleResourcePrefixesDeletedOnUninstall { get; } = new[]
    {
        "Admin.Expenses",
        "Enums.Nop.Plugin.Misc.Expenses.Domain.ExpenseCategoryType",
        $"Security.Permission.{PermissionProvider.ManageExpenseCategories}"
    };

    /// <summary>
    /// Gets the plugin's full set of locale resources (key to default English text). Public so the
    /// lifecycle tests can enumerate the installed keys.
    /// </summary>
    /// <returns>Locale resources</returns>
    public static IReadOnlyDictionary<string, string> GetLocaleResources()
    {
        const string p = "Admin.Expenses.";
        const string c = p + "ExpenseCategory.";

        return new Dictionary<string, string>
        {
            //admin menu (Infrastructure/AdminMenuManager.cs)
            [p + "Menu.Expenses"] = "Expenses",
            [p + "Menu.ExpenseCategory"] = "Expense Category",

            //page and grid
            [c + "PageTitle"] = "Expense categories",
            [c + "List.AddNew"] = "Add new",
            [c + "List.Name"] = "Name",
            [c + "List.PerMonthLimit"] = "Per month limit",
            [c + "List.Type"] = "Expense or income",
            [c + "List.Stores"] = "Stores",
            [c + "List.Stores.All"] = "All stores",
            [c + "List.Stores.Limited"] = "Limited to selected stores",
            [c + "List.EditStoresAndTranslations"] = "Edit stores and translations",

            //search panel
            [c + "Search.Name"] = "Name",
            [c + "Search.Store"] = "Store",
            [c + "Search.Store.All"] = "All stores",

            //model display names
            [c + "Fields.Name"] = "Name",
            [c + "Fields.PerMonthLimit"] = "Per month limit",
            [c + "Fields.Type"] = "Expense or income",
            [c + "Fields.Type.Select"] = "Select a type",
            [c + "Fields.LimitedToStores"] = "Limited to stores",
            [c + "Fields.SelectedStoreIds"] = "Limited to stores",
            [c + "Fields.Locales.Name"] = "Name (translation)",

            //stores and translations modal
            [c + "StoresAndTranslations.Title"] = "Stores and translations",
            [c + "StoresAndTranslations.Save"] = "Save",
            [c + "StoresAndTranslations.Cancel"] = "Cancel",
            [c + "StoresAndTranslations.StoresHint"] = "Select no store to make the category available in all stores. To select several stores, hold Ctrl (Command on Mac) while clicking, or use Ctrl+Arrow keys and Ctrl+Space from the keyboard.",
            [c + "StoresAndTranslations.DefaultNameHint"] = "Edit the default name inline in the grid. Leave a translation empty to use the default name.",

            //ExpenseCategoryType enum display (ILocalizationService.GetLocalizedEnumAsync)
            ["Enums.Nop.Plugin.Misc.Expenses.Domain.ExpenseCategoryType.Expense"] = "Expense",
            ["Enums.Nop.Plugin.Misc.Expenses.Domain.ExpenseCategoryType.Income"] = "Income",

            //validation and write-path errors (no NameDuplicate key: duplicate names are allowed)
            [c + "Validation.NameRequired"] = "Name is required.",
            //{0} = NopExpensesDefaults.NameMaxLength, formatted in at display time so the text cannot drift from the limit
            [c + "Validation.NameTooLong"] = "Name must not exceed {0} characters.",
            [c + "Validation.TypeRequired"] = "Select Expense or Income.",
            [c + "Validation.TypeLocked"] = "The type of an existing category cannot be changed.",
            [c + "Validation.PerMonthLimitInvalid"] = "Per month limit must be a number that is 0 or greater.",
            //{0} = NopExpensesDefaults.PerMonthLimitMaxValue
            [c + "Validation.PerMonthLimitTooLarge"] = "Per month limit must not exceed {0}.",
            //{0} = NopExpensesDefaults.PerMonthLimitMaxScale
            [c + "Validation.PerMonthLimitTooManyDecimals"] = "Per month limit must not have more than {0} decimal places.",
            [c + "Validation.RecordNotFound"] = "The expense category was not found.",
            [c + "Validation.InvalidRequest"] = "The submitted values are not valid.",
            [c + "Error.Generic"] = "The request could not be completed. Please try again.",

            //notifications
            [c + "Added"] = "The expense category has been added successfully.",
            [c + "Updated"] = "The expense category has been updated successfully.",
            [c + "StoresAndTranslations.Saved"] = "The stores and translations have been saved successfully.",

            //accessibility labels
            [c + "Accessibility.CloseDialog"] = "Close dialog",

            //permission display name ("Security.Permission.{SystemName}" is the framework's mechanism)
            [$"Security.Permission.{PermissionProvider.ManageExpenseCategories}"] = "Admin area. Manage expense categories"
        };
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            GetLocaleResources().ToDictionary(pair => pair.Key, pair => pair.Value));

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //the permission record (and its role mappings and localized name); there is no framework-automatic
        //counterpart to the insert-on-application-start
        await _permissionService.DeletePermissionAsync(PermissionProvider.ManageExpenseCategories);

        //core-table rows owned by this plugin's entity: dropping the table does not remove them (design 3.4).
        //Must run while the plugin is still loaded and the table still exists, i.e. before base/migration down
        await _storeMappingRepository.DeleteAsync(mapping => mapping.EntityName == nameof(ExpenseCategory));
        await _localizedPropertyRepository.DeleteAsync(property => property.LocaleKeyGroup == nameof(ExpenseCategory));

        //per-customer page preferences (collapsed panels) saved by the core Preferences/SavePreference action as
        //Customer generic attributes under the page's key prefix; the core does not remove them for a plugin
        await _genericAttributeRepository.DeleteAsync(attribute => attribute.KeyGroup == nameof(Customer)
            && attribute.Key.StartsWith(NopExpensesDefaults.PageAttributePrefix));

        //locale resources, symmetric with InstallAsync
        await _localizationService.DeleteLocaleResourcesAsync("Admin.Expenses");
        await _localizationService.DeleteLocaleResourcesAsync("Enums.Nop.Plugin.Misc.Expenses.Domain.ExpenseCategoryType");
        await _localizationService.DeleteLocaleResourceAsync($"Security.Permission.{PermissionProvider.ManageExpenseCategories}");

        //the table is dropped by the framework (auto-reversed migration) after this method returns
        await base.UninstallAsync();
    }

    #endregion
}
