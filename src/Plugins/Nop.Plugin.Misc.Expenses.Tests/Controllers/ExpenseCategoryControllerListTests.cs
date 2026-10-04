using Microsoft.AspNetCore.Mvc;
using Moq;
using Nop.Core;
using Nop.Core.Domain.Directory;
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
using Xunit;

namespace Nop.Plugin.Misc.Expenses.Tests.Controllers;

/// <summary>
/// Unit tests for the read path of <see cref="ExpenseCategoryController"/> (AC3.2, AC3.4, AC3.5)
/// </summary>
public class ExpenseCategoryControllerListTests
{
    private const int PrimaryCurrencyId = 7;

    private static readonly Currency PrimaryCurrency = new() { Id = PrimaryCurrencyId, CurrencyCode = "USD" };

    private sealed class Fixture
    {
        public Mock<IExpenseCategoryService> Service { get; } = new();
        public Mock<ICurrencyService> Currency { get; } = new();
        public Mock<IPriceFormatter> Formatter { get; } = new();
        public ExpenseCategoryController Controller { get; }

        public Fixture(IList<ExpenseCategory> categories)
        {
            Service
                .Setup(s => s.GetAllExpenseCategoriesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new PagedList<ExpenseCategory>(categories.ToList(), 0, 100, categories.Count));

            Currency.Setup(c => c.GetCurrencyByIdAsync(PrimaryCurrencyId)).ReturnsAsync(PrimaryCurrency);

            Formatter
                .Setup(f => f.FormatPriceAsync(It.IsAny<decimal>(), true, PrimaryCurrency))
                .ReturnsAsync((decimal price, bool _, Currency _) => $"${price:N2}");

            var localization = new Mock<ILocalizationService>();
            localization.Setup(l => l.GetResourceAsync(It.IsAny<string>())).ReturnsAsync((string key) => key);
            localization
                .Setup(l => l.GetLocalizedEnumAsync(It.IsAny<ExpenseCategoryType>(), It.IsAny<int?>()))
                .ReturnsAsync((ExpenseCategoryType t, int? _) => t.ToString());

            Controller = new ExpenseCategoryController(new CurrencySettings { PrimaryStoreCurrencyId = PrimaryCurrencyId },
                new Mock<IBaseAdminModelFactory>().Object, Currency.Object,
                new ExpenseCategoryValidator(localization.Object), Service.Object,
                localization.Object, Formatter.Object,
                new Mock<ILanguageService>().Object, new Mock<ILocalizedEntityService>().Object,
                new Mock<ILocalizedModelFactory>().Object, new Mock<IStoreMappingService>().Object,
                new Mock<IStoreMappingSupportedModelFactory>().Object, new Mock<IStoreService>().Object);
        }
    }

    private static ExpenseCategoryListModel ReadModel(IActionResult result)
    {
        var json = Assert.IsType<JsonResult>(result);
        return Assert.IsType<ExpenseCategoryListModel>(json.Value);
    }

    [Fact]
    public async Task ExpenseCategoryList_OversizedPageSize_IsClampedTo100BeforeServiceCall()
    {
        var f = new Fixture(new List<ExpenseCategory>());

        await f.Controller.ExpenseCategoryList(new ExpenseCategorySearchModel { Length = 5000 });

        f.Service.Verify(s => s.GetAllExpenseCategoriesAsync(null, 0, 0, NopExpensesDefaults.MaxPageSize), Times.Once);
    }

    [Fact]
    public async Task ExpenseCategoryList_ZeroLength_DoesNotThrowAndUsesMinimumPageSize()
    {
        var f = new Fixture(new List<ExpenseCategory>());

        await f.Controller.ExpenseCategoryList(new ExpenseCategorySearchModel { Length = 0 });

        f.Service.Verify(s => s.GetAllExpenseCategoriesAsync(null, 0, 0, 1), Times.Once);
    }

    [Fact]
    public async Task ExpenseCategoryList_PassesSearchFiltersAndPageIndex()
    {
        var f = new Fixture(new List<ExpenseCategory>());

        await f.Controller.ExpenseCategoryList(new ExpenseCategorySearchModel
        {
            SearchName = "office",
            SearchStoreId = 3,
            Start = 20,
            Length = 10
        });

        f.Service.Verify(s => s.GetAllExpenseCategoriesAsync("office", 3, 2, 10), Times.Once);
    }

    [Fact]
    public async Task ExpenseCategoryList_NullLimit_YieldsEmptyDisplay()
    {
        var f = new Fixture(new List<ExpenseCategory>
        {
            new() { Id = 1, Name = "A", PerMonthLimit = null, ExpenseCategoryType = ExpenseCategoryType.Expense }
        });

        var model = ReadModel(await f.Controller.ExpenseCategoryList(new ExpenseCategorySearchModel { Length = 10 }));

        Assert.Equal(string.Empty, model.Data.Single().PerMonthLimitDisplay);
        Assert.Null(model.Data.Single().PerMonthLimit);
        f.Formatter.Verify(p => p.FormatPriceAsync(It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<Currency>()), Times.Never);
    }

    [Fact]
    public async Task ExpenseCategoryList_ZeroLimit_IsFormattedNotEmpty()
    {
        var f = new Fixture(new List<ExpenseCategory>
        {
            new() { Id = 1, Name = "A", PerMonthLimit = 0m, ExpenseCategoryType = ExpenseCategoryType.Income }
        });

        var model = ReadModel(await f.Controller.ExpenseCategoryList(new ExpenseCategorySearchModel { Length = 10 }));

        Assert.Equal("$0.00", model.Data.Single().PerMonthLimitDisplay);
    }

    [Fact]
    public async Task ExpenseCategoryList_Limit_IsFormattedInPrimaryCurrencyResolvedOnce()
    {
        var f = new Fixture(new List<ExpenseCategory>
        {
            new() { Id = 1, Name = "A", PerMonthLimit = 1234.5m, ExpenseCategoryType = ExpenseCategoryType.Expense },
            new() { Id = 2, Name = "B", PerMonthLimit = 10m, ExpenseCategoryType = ExpenseCategoryType.Income, LimitedToStores = true }
        });

        var model = ReadModel(await f.Controller.ExpenseCategoryList(new ExpenseCategorySearchModel { Length = 10 }));

        Assert.Equal("$1,234.50", model.Data.First().PerMonthLimitDisplay);
        Assert.Equal("Income", model.Data.Last().ExpenseCategoryTypeName);
        Assert.True(model.Data.Last().LimitedToStores);
        f.Currency.Verify(c => c.GetCurrencyByIdAsync(PrimaryCurrencyId), Times.Once);
        f.Formatter.Verify(p => p.FormatPriceAsync(It.IsAny<decimal>(), true, PrimaryCurrency), Times.Exactly(2));
    }

    [Fact]
    public async Task ExpenseCategoryList_NoCategories_ReturnsEmptyPage()
    {
        var f = new Fixture(new List<ExpenseCategory>());

        var model = ReadModel(await f.Controller.ExpenseCategoryList(new ExpenseCategorySearchModel { Length = 10, Draw = "3" }));

        Assert.Empty(model.Data);
        Assert.Equal(0, model.RecordsTotal);
        Assert.Equal("3", model.Draw);
    }
}
