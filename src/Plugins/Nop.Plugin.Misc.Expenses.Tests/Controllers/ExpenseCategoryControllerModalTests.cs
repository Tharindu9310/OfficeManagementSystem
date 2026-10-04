using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.Expenses.Controllers;
using Nop.Plugin.Misc.Expenses.Domain;
using Nop.Plugin.Misc.Expenses.Models.Admin;
using Nop.Plugin.Misc.Expenses.Services;
using Nop.Plugin.Misc.Expenses.Validators;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Mvc;
using Xunit;

namespace Nop.Plugin.Misc.Expenses.Tests.Controllers;

/// <summary>
/// Unit tests for the "Edit stores and translations" modal actions of <see cref="ExpenseCategoryController"/>
/// (AC5.3, AC5.5 to AC5.9, AC5.10 is covered by the permission coverage tests)
/// </summary>
public class ExpenseCategoryControllerModalTests
{
    private const string Prefix = "Admin.Expenses.ExpenseCategory.Validation.";

    private sealed class Fixture
    {
        public Mock<IExpenseCategoryService> Service { get; } = new();
        public Mock<IStoreMappingService> StoreMapping { get; } = new();
        public Mock<ILocalizedEntityService> LocalizedEntity { get; } = new();
        public Mock<ILocalizationService> Localization { get; } = new();
        public Mock<IStoreMappingSupportedModelFactory> StoreModelFactory { get; } = new();
        public Mock<ILocalizedModelFactory> LocalizedModelFactory { get; } = new();
        public ExpenseCategoryController Controller { get; }
        public ExpenseCategory Stored { get; }

        public Fixture(ExpenseCategory stored = null, int[] storeIds = null, int[] languageIds = null)
        {
            Stored = stored;
            storeIds ??= new[] { 1, 2 };
            languageIds ??= new[] { 1, 2 };

            Service.Setup(s => s.GetExpenseCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => Stored != null && Stored.Id == id ? Stored : null);

            Localization.Setup(l => l.GetResourceAsync(It.IsAny<string>())).ReturnsAsync((string key) => key);

            var storeService = new Mock<IStoreService>();
            storeService.Setup(s => s.GetAllStoresAsync())
                .ReturnsAsync(storeIds.Select(id => new Store { Id = id, Name = "Store " + id }).ToList());

            var languages = languageIds.Select(id => new Language { Id = id, Name = "Language " + id }).ToList();
            var languageService = new Mock<ILanguageService>();
            languageService.Setup(s => s.GetAllLanguagesAsync(It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(languages);
            languageService.Setup(s => s.GetLanguageByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => languages.FirstOrDefault(language => language.Id == id));

            //run the configure callback for each language, like the framework factory does
            LocalizedModelFactory
                .Setup(f => f.PrepareLocalizedModelsAsync(It.IsAny<Func<ExpenseCategoryLocalizedModel, int, Task>>()))
                .Returns(async (Func<ExpenseCategoryLocalizedModel, int, Task> configure) =>
                {
                    IList<ExpenseCategoryLocalizedModel> models = new List<ExpenseCategoryLocalizedModel>();
                    foreach (var id in languageIds)
                    {
                        var model = new ExpenseCategoryLocalizedModel { LanguageId = id };
                        await configure(model, id);
                        models.Add(model);
                    }

                    return models;
                });

            Controller = new ExpenseCategoryController(new CurrencySettings(),
                new Mock<IBaseAdminModelFactory>().Object, new Mock<ICurrencyService>().Object,
                new ExpenseCategoryValidator(Localization.Object), Service.Object,
                Localization.Object, new Mock<IPriceFormatter>().Object,
                languageService.Object, LocalizedEntity.Object, LocalizedModelFactory.Object,
                StoreMapping.Object, StoreModelFactory.Object, storeService.Object);
        }

        public void VerifyNothingStored()
        {
            StoreMapping.Verify(s => s.SaveStoreMappingsAsync(It.IsAny<ExpenseCategory>(), It.IsAny<IEnumerable<int>>()), Times.Never);
            LocalizedEntity.Verify(s => s.SaveLocalizedValueAsync(It.IsAny<ExpenseCategory>(),
                It.IsAny<Expression<Func<ExpenseCategory, string>>>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            Service.Verify(s => s.UpdateExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
        }
    }

    private static ExpenseCategory Stored() => new()
    {
        Id = 5,
        Name = "Original",
        PerMonthLimit = 100m,
        ExpenseCategoryType = ExpenseCategoryType.Income,
        LimitedToStores = false
    };

    private static ExpenseCategoryStoresAndTranslationsModel PostedModel(IList<int> storeIds, params (int LanguageId, string Name)[] locales) => new()
    {
        Id = 5,
        //posted values for these must be ignored: the modal never changes the stored name
        Name = "Hacked",
        SelectedStoreIds = storeIds,
        Locales = locales.Select(l => new ExpenseCategoryLocalizedModel { LanguageId = l.LanguageId, Name = l.Name }).ToList()
    };

    private static IDictionary<string, string> FieldErrors(IActionResult result)
    {
        var json = Assert.IsType<JsonResult>(result);
        var property = json.Value.GetType().GetProperty("fieldErrors", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);

        return Assert.IsAssignableFrom<IDictionary<string, string>>(property.GetValue(json.Value));
    }

    private static string ErrorText(IActionResult result)
    {
        var json = Assert.IsType<JsonResult>(result);
        return (string)json.Value.GetType().GetProperty("error")!.GetValue(json.Value);
    }

    #region Save

    [Fact]
    public async Task Save_StoresAndTranslations_CallsBothServicesAndLeavesNameLimitAndTypeUntouched()
    {
        var f = new Fixture(Stored());

        var result = await f.Controller.SaveStoresAndTranslations(PostedModel(new List<int> { 2 }, (1, "  Reisen "), (2, "Voyages")));

        Assert.IsType<NullJsonResult>(result);
        f.StoreMapping.Verify(s => s.SaveStoreMappingsAsync(f.Stored, It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 2 }))), Times.Once);
        f.LocalizedEntity.Verify(s => s.SaveLocalizedValueAsync(f.Stored, It.IsAny<Expression<Func<ExpenseCategory, string>>>(), "Reisen", 1), Times.Once);
        f.LocalizedEntity.Verify(s => s.SaveLocalizedValueAsync(f.Stored, It.IsAny<Expression<Func<ExpenseCategory, string>>>(), "Voyages", 2), Times.Once);
        f.Service.Verify(s => s.UpdateExpenseCategoryAsync(f.Stored), Times.Once);

        Assert.Equal("Original", f.Stored.Name);
        Assert.Equal(100m, f.Stored.PerMonthLimit);
        Assert.Equal(ExpenseCategoryType.Income, f.Stored.ExpenseCategoryType);
    }

    [Fact]
    public async Task Save_TranslationNameKeyTargetsTheNameProperty()
    {
        var f = new Fixture(Stored());
        Expression<Func<ExpenseCategory, string>> captured = null;
        f.LocalizedEntity
            .Setup(s => s.SaveLocalizedValueAsync(It.IsAny<ExpenseCategory>(), It.IsAny<Expression<Func<ExpenseCategory, string>>>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<ExpenseCategory, Expression<Func<ExpenseCategory, string>>, string, int>((_, key, _, _) => captured = key)
            .Returns(Task.CompletedTask);

        await f.Controller.SaveStoresAndTranslations(PostedModel(new List<int>(), (1, "x")));

        Assert.NotNull(captured);
        Assert.Equal(nameof(ExpenseCategory.Name), ((MemberExpression)captured.Body).Member.Name);
    }

    [Fact]
    public async Task Save_NoStoresSelected_SavesEmptyStoreListSoCategoryIsAvailableInAllStores()
    {
        var f = new Fixture(Stored());

        var result = await f.Controller.SaveStoresAndTranslations(PostedModel(null));

        Assert.IsType<NullJsonResult>(result);
        f.StoreMapping.Verify(s => s.SaveStoreMappingsAsync(f.Stored, It.Is<IEnumerable<int>>(ids => !ids.Any())), Times.Once);
    }

    [Fact]
    public async Task Save_UnknownStoreIds_AreDroppedSoTheCategoryIsNotLimitedToNoStore()
    {
        var f = new Fixture(Stored());

        await f.Controller.SaveStoresAndTranslations(PostedModel(new List<int> { 99, 1, 1 }));

        f.StoreMapping.Verify(s => s.SaveStoreMappingsAsync(f.Stored, It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 1 }))), Times.Once);
    }

    [Fact]
    public async Task Save_OnlyUnknownStoreIds_SavesEmptyStoreList()
    {
        var f = new Fixture(Stored());

        await f.Controller.SaveStoresAndTranslations(PostedModel(new List<int> { 99 }));

        f.StoreMapping.Verify(s => s.SaveStoreMappingsAsync(f.Stored, It.Is<IEnumerable<int>>(ids => !ids.Any())), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Save_EmptyTranslation_IsPassedAsEmptySoTheServiceRemovesItAndTheDefaultNameApplies(string value)
    {
        var f = new Fixture(Stored());

        var result = await f.Controller.SaveStoresAndTranslations(PostedModel(new List<int>(), (1, value)));

        Assert.IsType<NullJsonResult>(result);
        f.LocalizedEntity.Verify(s => s.SaveLocalizedValueAsync(f.Stored, It.IsAny<Expression<Func<ExpenseCategory, string>>>(), string.Empty, 1), Times.Once);
    }

    [Fact]
    public async Task Save_TranslationOf200Characters_IsAccepted()
    {
        var f = new Fixture(Stored());
        var value = new string('a', NopExpensesDefaults.NameMaxLength);

        var result = await f.Controller.SaveStoresAndTranslations(PostedModel(new List<int>(), (1, value)));

        Assert.IsType<NullJsonResult>(result);
        f.LocalizedEntity.Verify(s => s.SaveLocalizedValueAsync(f.Stored, It.IsAny<Expression<Func<ExpenseCategory, string>>>(), value, 1), Times.Once);
    }

    [Fact]
    public async Task Save_TranslationOver200Characters_IsRejectedAndNothingFromThatSaveIsStored()
    {
        var f = new Fixture(Stored());

        //the first language is valid and the second is too long: even the valid one must not be stored
        var result = await f.Controller.SaveStoresAndTranslations(PostedModel(new List<int> { 1 },
            (1, "Fine"), (2, new string('a', NopExpensesDefaults.NameMaxLength + 1))));

        var errors = FieldErrors(result);
        Assert.Single(errors);
        Assert.Equal(Prefix + "NameTooLong", errors["Locales[1].Name"]);
        f.VerifyNothingStored();
    }

    [Fact]
    public async Task Save_TranslationOver200CharactersAfterTrimming_CountsTheTrimmedLength()
    {
        var f = new Fixture(Stored());
        var padded = "  " + new string('a', NopExpensesDefaults.NameMaxLength) + "  ";

        var result = await f.Controller.SaveStoresAndTranslations(PostedModel(new List<int>(), (1, padded)));

        Assert.IsType<NullJsonResult>(result);
    }

    [Fact]
    public async Task Save_UnknownLanguage_IsIgnoredAndNoOrphanTranslationIsStored()
    {
        var f = new Fixture(Stored());

        var result = await f.Controller.SaveStoresAndTranslations(PostedModel(new List<int>(), (1, "Ok"), (77, "Orphan")));

        Assert.IsType<NullJsonResult>(result);
        f.LocalizedEntity.Verify(s => s.SaveLocalizedValueAsync(It.IsAny<ExpenseCategory>(),
            It.IsAny<Expression<Func<ExpenseCategory, string>>>(), It.IsAny<string>(), 77), Times.Never);
        f.LocalizedEntity.Verify(s => s.SaveLocalizedValueAsync(f.Stored,
            It.IsAny<Expression<Func<ExpenseCategory, string>>>(), "Ok", 1), Times.Once);
    }

    [Fact]
    public async Task Save_CategoryNotFound_ReturnsLocalizedErrorAndStoresNothing()
    {
        var f = new Fixture(Stored());
        var model = PostedModel(new List<int> { 1 }, (1, "x"));
        model.Id = 99;

        var result = await f.Controller.SaveStoresAndTranslations(model);

        Assert.Equal(Prefix + "RecordNotFound", ErrorText(result));
        f.VerifyNothingStored();
    }

    [Fact]
    public async Task Save_InvalidModelState_ReturnsErrorAndStoresNothing()
    {
        var f = new Fixture(Stored());
        f.Controller.ModelState.AddModelError("Locales[0].LanguageId", "The value 'abc' is not valid.");

        var result = await f.Controller.SaveStoresAndTranslations(PostedModel(new List<int> { 1 }, (1, "x")));

        Assert.Equal(Prefix + "InvalidRequest", ErrorText(result));
        f.VerifyNothingStored();
    }

    #endregion

    #region Load

    [Fact]
    public async Task Edit_ExistingCategory_ReturnsPartialWithStoredNameStoresAndPerLanguageValues()
    {
        var f = new Fixture(Stored());
        f.Localization
            .Setup(l => l.GetLocalizedAsync(f.Stored, It.IsAny<Expression<Func<ExpenseCategory, string>>>(), 1, false, false))
            .ReturnsAsync("Reisen");
        f.Localization
            .Setup(l => l.GetLocalizedAsync(f.Stored, It.IsAny<Expression<Func<ExpenseCategory, string>>>(), 2, false, false))
            .ReturnsAsync((string)null);

        var result = await f.Controller.EditStoresAndTranslations(5);

        var partial = Assert.IsType<PartialViewResult>(result);
        var model = Assert.IsType<ExpenseCategoryStoresAndTranslationsModel>(partial.Model);
        Assert.Equal(5, model.Id);
        Assert.Equal("Original", model.Name);
        Assert.Equal(2, model.Locales.Count);
        Assert.Equal("Reisen", model.Locales[0].Name);
        Assert.Equal("Language 1", model.Locales[0].LanguageName);
        //no translation: shown empty, never the default name
        Assert.Equal(string.Empty, model.Locales[1].Name);
        f.StoreModelFactory.Verify(m => m.PrepareModelStoresAsync(model, f.Stored, false), Times.Once);
    }

    [Fact]
    public async Task Edit_MissingCategory_ReturnsLocalizedError()
    {
        var f = new Fixture(Stored());

        var result = await f.Controller.EditStoresAndTranslations(99);

        Assert.Equal(Prefix + "RecordNotFound", ErrorText(result));
    }

    #endregion
}
