using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Misc.Expenses.Domain;
using Nop.Plugin.Misc.Expenses.Infrastructure;
using Nop.Plugin.Misc.Expenses.Models.Admin;
using Nop.Plugin.Misc.Expenses.Services;
using Nop.Plugin.Misc.Expenses.Validators;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Expenses.Controllers;

/// <summary>
/// Represents the Expense Category admin controller. Every action is gated by
/// <see cref="PermissionProvider.ManageExpenseCategories"/>. There is deliberately no delete action.
/// </summary>
[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class ExpenseCategoryController : BasePluginController
{
    #region Fields

    protected readonly CurrencySettings _currencySettings;
    protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
    protected readonly ICurrencyService _currencyService;
    protected readonly ExpenseCategoryValidator _expenseCategoryValidator;
    protected readonly IExpenseCategoryService _expenseCategoryService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IPriceFormatter _priceFormatter;
    protected readonly ILanguageService _languageService;
    protected readonly ILocalizedEntityService _localizedEntityService;
    protected readonly ILocalizedModelFactory _localizedModelFactory;
    protected readonly IStoreMappingService _storeMappingService;
    protected readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    protected readonly IStoreService _storeService;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpenseCategoryController"/> class
    /// </summary>
    /// <param name="currencySettings">Currency settings</param>
    /// <param name="baseAdminModelFactory">Base admin model factory</param>
    /// <param name="currencyService">Currency service</param>
    /// <param name="expenseCategoryValidator">Expense category validator</param>
    /// <param name="expenseCategoryService">Expense category service</param>
    /// <param name="localizationService">Localization service</param>
    /// <param name="priceFormatter">Price formatter</param>
    /// <param name="languageService">Language service</param>
    /// <param name="localizedEntityService">Localized entity service</param>
    /// <param name="localizedModelFactory">Localized model factory</param>
    /// <param name="storeMappingService">Store mapping service</param>
    /// <param name="storeMappingSupportedModelFactory">Store mapping supported model factory</param>
    /// <param name="storeService">Store service</param>
    public ExpenseCategoryController(CurrencySettings currencySettings,
        IBaseAdminModelFactory baseAdminModelFactory,
        ICurrencyService currencyService,
        ExpenseCategoryValidator expenseCategoryValidator,
        IExpenseCategoryService expenseCategoryService,
        ILocalizationService localizationService,
        IPriceFormatter priceFormatter,
        ILanguageService languageService,
        ILocalizedEntityService localizedEntityService,
        ILocalizedModelFactory localizedModelFactory,
        IStoreMappingService storeMappingService,
        IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
        IStoreService storeService)
    {
        _currencySettings = currencySettings;
        _baseAdminModelFactory = baseAdminModelFactory;
        _currencyService = currencyService;
        _expenseCategoryValidator = expenseCategoryValidator;
        _expenseCategoryService = expenseCategoryService;
        _localizationService = localizationService;
        _priceFormatter = priceFormatter;
        _languageService = languageService;
        _localizedEntityService = localizedEntityService;
        _localizedModelFactory = localizedModelFactory;
        _storeMappingService = storeMappingService;
        _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        _storeService = storeService;
    }

    #endregion

    #region Utilities

    private async Task<ExpenseCategoryModel> PrepareExpenseCategoryModelAsync(ExpenseCategory category, Currency primaryCurrency)
    {
        string limitDisplay = null;

        //a null limit renders as an empty cell (never 0)
        if (category.PerMonthLimit.HasValue)
        {
            limitDisplay = primaryCurrency != null
                ? await _priceFormatter.FormatPriceAsync(category.PerMonthLimit.Value, true, primaryCurrency)
                : category.PerMonthLimit.Value.ToString("N2");
        }

        return new ExpenseCategoryModel
        {
            Id = category.Id,
            Name = category.Name,
            PerMonthLimit = category.PerMonthLimit,
            PerMonthLimitDisplay = limitDisplay ?? string.Empty,
            ExpenseCategoryTypeId = category.ExpenseCategoryTypeId,
            ExpenseCategoryTypeName = await _localizationService.GetLocalizedEnumAsync(category.ExpenseCategoryType),
            LimitedToStores = category.LimitedToStores
        };
    }

    /// <summary>
    /// Validates the candidate entity with the shared validator and maps the posted model's binding
    /// errors (a non-numeric Per Month Limit) to the same localized field error
    /// </summary>
    /// <param name="candidate">Entity built from the posted values</param>
    /// <returns>Field-keyed messages (key = property name), or null when valid</returns>
    private async Task<Dictionary<string, string>> ValidateExpenseCategoryAsync(ExpenseCategory candidate)
    {
        var validationResult = await _expenseCategoryValidator.ValidateAsync(candidate);

        //only the first message per field is kept
        var fieldErrors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.First().ErrorMessage);

        return fieldErrors.Count > 0 ? fieldErrors : null;
    }

    /// <summary>
    /// Checks the model binding result first (before any lookup, validation or write). A value that failed to
    /// bind (the bound property stays at its default, so the validator cannot see it) becomes the same localized
    /// field error the validator would give; a failure on any other key is a generic localized error.
    /// </summary>
    /// <returns>The error result, or null when model binding succeeded</returns>
    private async Task<IActionResult> RejectInvalidModelStateAsync()
    {
        if (ModelState.IsValid)
            return null;

        const string prefix = "Admin.Expenses.ExpenseCategory.Validation.";

        //posted key -> the validator-equivalent message key
        var messageKeys = new Dictionary<string, string>
        {
            [nameof(ExpenseCategoryModel.PerMonthLimit)] = prefix + "PerMonthLimitInvalid",
            [nameof(ExpenseCategoryModel.ExpenseCategoryTypeId)] = prefix + "TypeRequired",
            [nameof(ExpenseCategoryModel.Name)] = prefix + "NameRequired"
        };

        var fieldErrors = new Dictionary<string, string>();

        foreach (var (key, messageKey) in messageKeys)
        {
            if (ModelState.TryGetValue(key, out var state) && state.Errors.Count > 0)
                fieldErrors[key] = await _localizationService.GetResourceAsync(messageKey);
        }

        if (fieldErrors.Count > 0)
            return Json(new { fieldErrors });

        return ErrorJson(await _localizationService.GetResourceAsync(prefix + "InvalidRequest"));
    }

    #endregion

    #region Methods

    /// <summary>
    /// Displays the Expense Category page (search panel and grid)
    /// </summary>
    /// <returns>The List view</returns>
    [CheckPermission(PermissionProvider.ManageExpenseCategories)]
    public virtual async Task<IActionResult> List()
    {
        var searchModel = new ExpenseCategorySearchModel();
        searchModel.SetGridPageSize();

        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores,
            defaultItemText: await _localizationService.GetResourceAsync("Admin.Expenses.ExpenseCategory.Search.Store.All"));

        return View("~/Plugins/Misc.Expenses/Views/ExpenseCategory/List.cshtml", searchModel);
    }

    /// <summary>
    /// Reads one page of expense categories for the grid (page size is clamped server-side)
    /// </summary>
    /// <param name="searchModel">Search model</param>
    /// <returns>JSON grid model</returns>
    [HttpPost]
    [CheckPermission(PermissionProvider.ManageExpenseCategories)]
    public virtual async Task<IActionResult> ExpenseCategoryList(ExpenseCategorySearchModel searchModel)
    {
        //guard against a zero/negative length (Page divides by Length) and clamp to the maximum page size
        var pageIndex = searchModel.Length > 0 ? Math.Max(searchModel.Page - 1, 0) : 0;
        var pageSize = Math.Clamp(searchModel.Length, 1, NopExpensesDefaults.MaxPageSize);

        var categories = await _expenseCategoryService.GetAllExpenseCategoriesAsync(searchModel.SearchName,
            searchModel.SearchStoreId, pageIndex, pageSize);

        //resolve the primary currency once per request (no N+1)
        var primaryCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

        var model = await new ExpenseCategoryListModel().PrepareToGridAsync(searchModel, categories, () =>
        {
            return categories.SelectAwait(async category => await PrepareExpenseCategoryModelAsync(category, primaryCurrency));
        });

        return Json(model);
    }

    /// <summary>
    /// Adds a category. It is available in all stores and has no translations until they are set in the modal
    /// </summary>
    /// <param name="model">Posted values (Name, Per Month Limit, type)</param>
    /// <returns>An empty result, or the field-keyed validation errors</returns>
    [HttpPost]
    [CheckPermission(PermissionProvider.ManageExpenseCategories)]
    public virtual async Task<IActionResult> ExpenseCategoryInsert(ExpenseCategoryModel model)
    {
        var invalidModelState = await RejectInvalidModelStateAsync();

        if (invalidModelState != null)
            return invalidModelState;

        var category = new ExpenseCategory
        {
            Name = model.Name?.Trim(),
            PerMonthLimit = model.PerMonthLimit,
            ExpenseCategoryTypeId = model.ExpenseCategoryTypeId,
            LimitedToStores = false
        };

        var fieldErrors = await ValidateExpenseCategoryAsync(category);

        if (fieldErrors != null)
            return Json(new { fieldErrors });

        await _expenseCategoryService.InsertExpenseCategoryAsync(category);

        return new NullJsonResult();
    }

    /// <summary>
    /// Updates the Name and Per Month Limit of a category. The type is never taken from the posted
    /// model: a different posted type is rejected and nothing is saved
    /// </summary>
    /// <param name="model">Posted values (Id, Name, Per Month Limit and the current type)</param>
    /// <returns>An empty result, the field-keyed validation errors, or an error message</returns>
    [HttpPost]
    [CheckPermission(PermissionProvider.ManageExpenseCategories)]
    public virtual async Task<IActionResult> ExpenseCategoryUpdate(ExpenseCategoryModel model)
    {
        var invalidModelState = await RejectInvalidModelStateAsync();

        if (invalidModelState != null)
            return invalidModelState;

        var category = await _expenseCategoryService.GetExpenseCategoryByIdAsync(model.Id);

        if (category == null)
            return ErrorJson(await _localizationService.GetResourceAsync("Admin.Expenses.ExpenseCategory.Validation.RecordNotFound"));

        //type lock: an equal value proceeds, anything else (including a missing value) is rejected
        if (model.ExpenseCategoryTypeId != category.ExpenseCategoryTypeId)
            return ErrorJson(await _localizationService.GetResourceAsync("Admin.Expenses.ExpenseCategory.Validation.TypeLocked"));

        //validate a candidate so a rejected request never leaves the loaded entity modified
        var candidate = new ExpenseCategory
        {
            Name = model.Name?.Trim(),
            PerMonthLimit = model.PerMonthLimit,
            ExpenseCategoryTypeId = category.ExpenseCategoryTypeId
        };

        var fieldErrors = await ValidateExpenseCategoryAsync(candidate);

        if (fieldErrors != null)
            return Json(new { fieldErrors });

        category.Name = candidate.Name;
        category.PerMonthLimit = candidate.PerMonthLimit;

        await _expenseCategoryService.UpdateExpenseCategoryAsync(category);

        return new NullJsonResult();
    }

    /// <summary>
    /// Loads the "Edit stores and translations" modal body for a category
    /// </summary>
    /// <param name="id">Expense category identifier</param>
    /// <returns>The modal body partial view, or an error JSON when the category does not exist</returns>
    [CheckPermission(PermissionProvider.ManageExpenseCategories)]
    public virtual async Task<IActionResult> EditStoresAndTranslations(int id)
    {
        var category = await _expenseCategoryService.GetExpenseCategoryByIdAsync(id);

        if (category == null)
            return ErrorJson(await _localizationService.GetResourceAsync("Admin.Expenses.ExpenseCategory.Validation.RecordNotFound"));

        var model = new ExpenseCategoryStoresAndTranslationsModel
        {
            Id = category.Id,
            Name = category.Name
        };

        //one translated name per language; an empty value means the default name is used
        model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync<ExpenseCategoryLocalizedModel>(async (locale, languageId) =>
        {
            locale.Name = await _localizationService.GetLocalizedAsync(category, entity => entity.Name, languageId,
                returnDefaultValue: false, ensureTwoPublishedLanguages: false) ?? string.Empty;
            locale.LanguageName = (await _languageService.GetLanguageByIdAsync(languageId))?.Name;
        });

        await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, category, false);

        return PartialView("~/Plugins/Misc.Expenses/Views/ExpenseCategory/_StoresAndTranslationsBody.cshtml", model);
    }

    /// <summary>
    /// Saves the stores and the translated names of a category. The stored default Name, the Per Month
    /// Limit and the type are never changed here
    /// </summary>
    /// <param name="model">Posted values (Id, selected store ids, translated names)</param>
    /// <returns>An empty result, the field-keyed validation errors (key = "Locales[i].Name"), or an error message</returns>
    [HttpPost]
    [CheckPermission(PermissionProvider.ManageExpenseCategories)]
    public virtual async Task<IActionResult> SaveStoresAndTranslations(ExpenseCategoryStoresAndTranslationsModel model)
    {
        //first, before any lookup or write; a localized message instead of the raw framework binding text
        if (!ModelState.IsValid)
            return ErrorJson(await _localizationService.GetResourceAsync("Admin.Expenses.ExpenseCategory.Validation.InvalidRequest"));

        var category = await _expenseCategoryService.GetExpenseCategoryByIdAsync(model.Id);

        if (category == null)
            return ErrorJson(await _localizationService.GetResourceAsync("Admin.Expenses.ExpenseCategory.Validation.RecordNotFound"));

        //validate every translation first so a rejected request stores nothing
        var languageIds = (await _languageService.GetAllLanguagesAsync(true)).Select(language => language.Id).ToHashSet();
        var translations = new List<(int LanguageId, string Value)>();
        var fieldErrors = new Dictionary<string, string>();

        for (var index = 0; index < (model.Locales?.Count ?? 0); index++)
        {
            var locale = model.Locales[index];

            //an unknown language is ignored rather than stored as an orphan row
            if (locale == null || !languageIds.Contains(locale.LanguageId))
                continue;

            var value = locale.Name?.Trim() ?? string.Empty;

            if (value.Length > NopExpensesDefaults.NameMaxLength)
            {
                fieldErrors[$"Locales[{index}].{nameof(ExpenseCategoryLocalizedModel.Name)}"] = string.Format(
                    CultureInfo.CurrentCulture,
                    await _localizationService.GetResourceAsync("Admin.Expenses.ExpenseCategory.Validation.NameTooLong"),
                    NopExpensesDefaults.NameMaxLength);
                continue;
            }

            translations.Add((locale.LanguageId, value));
        }

        if (fieldErrors.Count > 0)
            return Json(new { fieldErrors });

        //only existing stores count: an unknown id must not leave the category limited to no store
        var storeIds = (await _storeService.GetAllStoresAsync()).Select(store => store.Id)
            .Intersect(model.SelectedStoreIds ?? new List<int>()).ToList();

        //also sets LimitedToStores (true when any store is selected, false when none)
        await _storeMappingService.SaveStoreMappingsAsync(category, storeIds);

        //an empty value removes the translation, so the default name is used
        foreach (var (languageId, value) in translations)
            await _localizedEntityService.SaveLocalizedValueAsync(category, entity => entity.Name, value, languageId);

        //refresh UpdatedOnUtc and the plugin cache prefix
        await _expenseCategoryService.UpdateExpenseCategoryAsync(category);

        return new NullJsonResult();
    }

    #endregion
}
