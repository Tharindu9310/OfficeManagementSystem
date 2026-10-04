using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
using Nop.Web.Framework.Mvc;
using Xunit;

namespace Nop.Plugin.Misc.Expenses.Tests.Controllers;

/// <summary>
/// Unit tests for the insert and update actions of <see cref="ExpenseCategoryController"/>
/// (AC4.1, AC4.5, AC4.8, AC4.9, AC4.13)
/// </summary>
public class ExpenseCategoryControllerEditTests
{
    private const string Prefix = "Admin.Expenses.ExpenseCategory.Validation.";

    private sealed class Fixture
    {
        public Mock<IExpenseCategoryService> Service { get; } = new();
        public ExpenseCategoryController Controller { get; }
        public ExpenseCategory Stored { get; }

        public Fixture(ExpenseCategory stored = null)
        {
            Stored = stored;

            Service.Setup(s => s.GetExpenseCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => Stored != null && Stored.Id == id ? Stored : null);

            var localization = new Mock<ILocalizationService>();
            localization.Setup(l => l.GetResourceAsync(It.IsAny<string>())).ReturnsAsync((string key) => key);

            Controller = new ExpenseCategoryController(new CurrencySettings(),
                new Mock<IBaseAdminModelFactory>().Object, new Mock<ICurrencyService>().Object,
                new ExpenseCategoryValidator(localization.Object), Service.Object,
                localization.Object, new Mock<IPriceFormatter>().Object,
                new Mock<ILanguageService>().Object, new Mock<ILocalizedEntityService>().Object,
                new Mock<ILocalizedModelFactory>().Object, new Mock<IStoreMappingService>().Object,
                new Mock<IStoreMappingSupportedModelFactory>().Object, new Mock<IStoreService>().Object);
        }
    }

    private static ExpenseCategory Stored(ExpenseCategoryType type = ExpenseCategoryType.Expense) => new()
    {
        Id = 5,
        Name = "Original",
        PerMonthLimit = 100m,
        ExpenseCategoryType = type,
        LimitedToStores = true
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

    #region Insert

    [Fact]
    public async Task Insert_ValidModel_PersistsTrimmedNameAllStoresAndReturnsNullJson()
    {
        var f = new Fixture();
        ExpenseCategory saved = null;
        f.Service.Setup(s => s.InsertExpenseCategoryAsync(It.IsAny<ExpenseCategory>()))
            .Callback<ExpenseCategory>(c => saved = c).Returns(Task.CompletedTask);

        var result = await f.Controller.ExpenseCategoryInsert(new ExpenseCategoryModel
        {
            Name = "  Travel  ",
            PerMonthLimit = 250.5m,
            ExpenseCategoryTypeId = (int)ExpenseCategoryType.Income,
            LimitedToStores = true //must be ignored: a new category is available in all stores
        });

        Assert.IsType<NullJsonResult>(result);
        Assert.NotNull(saved);
        Assert.False(saved.LimitedToStores);
        Assert.Equal("Travel", saved.Name);
        Assert.Equal(250.5m, saved.PerMonthLimit);
        Assert.Equal(ExpenseCategoryType.Income, saved.ExpenseCategoryType);
    }

    [Fact]
    public async Task Insert_EmptyLimit_Succeeds()
    {
        var f = new Fixture();

        var result = await f.Controller.ExpenseCategoryInsert(new ExpenseCategoryModel
        {
            Name = "Travel",
            PerMonthLimit = null,
            ExpenseCategoryTypeId = (int)ExpenseCategoryType.Expense
        });

        Assert.IsType<NullJsonResult>(result);
        f.Service.Verify(s => s.InsertExpenseCategoryAsync(It.Is<ExpenseCategory>(c => c.PerMonthLimit == null)), Times.Once);
    }

    [Fact]
    public async Task Insert_MissingType_ReturnsTypeRequiredAndSavesNothing()
    {
        var f = new Fixture();

        var result = await f.Controller.ExpenseCategoryInsert(new ExpenseCategoryModel { Name = "Travel", ExpenseCategoryTypeId = 0 });

        Assert.Equal(Prefix + "TypeRequired", FieldErrors(result)[nameof(ExpenseCategory.ExpenseCategoryTypeId)]);
        f.Service.Verify(s => s.InsertExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Insert_BlankName_ReturnsNameRequiredAndSavesNothing(string name)
    {
        var f = new Fixture();

        var result = await f.Controller.ExpenseCategoryInsert(new ExpenseCategoryModel { Name = name, ExpenseCategoryTypeId = 1 });

        Assert.Equal(Prefix + "NameRequired", FieldErrors(result)[nameof(ExpenseCategory.Name)]);
        f.Service.Verify(s => s.InsertExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
    }

    [Fact]
    public async Task Insert_NegativeLimit_ReturnsLimitErrorAndSavesNothing()
    {
        var f = new Fixture();

        var result = await f.Controller.ExpenseCategoryInsert(new ExpenseCategoryModel { Name = "A", PerMonthLimit = -1m, ExpenseCategoryTypeId = 1 });

        Assert.Equal(Prefix + "PerMonthLimitInvalid", FieldErrors(result)[nameof(ExpenseCategory.PerMonthLimit)]);
        f.Service.Verify(s => s.InsertExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
    }

    [Fact]
    public async Task Insert_NonNumericLimitModelStateError_MapsToLimitFieldError()
    {
        var f = new Fixture();
        f.Controller.ModelState.AddModelError(nameof(ExpenseCategoryModel.PerMonthLimit), "The value 'abc' is not valid.");

        var result = await f.Controller.ExpenseCategoryInsert(new ExpenseCategoryModel { Name = "A", PerMonthLimit = null, ExpenseCategoryTypeId = 1 });

        //the localized message, never the raw framework binding message
        Assert.Equal(Prefix + "PerMonthLimitInvalid", FieldErrors(result)[nameof(ExpenseCategory.PerMonthLimit)]);
        f.Service.Verify(s => s.InsertExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_NotFound_ReturnsLocalizedErrorAndSavesNothing()
    {
        var f = new Fixture();

        var result = await f.Controller.ExpenseCategoryUpdate(new ExpenseCategoryModel { Id = 99, Name = "A", ExpenseCategoryTypeId = 1 });

        Assert.Equal(Prefix + "RecordNotFound", ErrorText(result));
        f.Service.Verify(s => s.UpdateExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
    }

    [Fact]
    public async Task Update_DifferentType_ReturnsTypeLockedAndSavesNothing()
    {
        var f = new Fixture(Stored(ExpenseCategoryType.Expense));

        var result = await f.Controller.ExpenseCategoryUpdate(new ExpenseCategoryModel
        {
            Id = 5,
            Name = "Changed",
            PerMonthLimit = 1m,
            ExpenseCategoryTypeId = (int)ExpenseCategoryType.Income
        });

        Assert.Equal(Prefix + "TypeLocked", ErrorText(result));
        f.Service.Verify(s => s.UpdateExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
        Assert.Equal("Original", f.Stored.Name);
        Assert.Equal(ExpenseCategoryType.Expense, f.Stored.ExpenseCategoryType);
    }

    [Fact]
    public async Task Update_MissingType_IsRejectedAsTypeLocked()
    {
        var f = new Fixture(Stored());

        var result = await f.Controller.ExpenseCategoryUpdate(new ExpenseCategoryModel { Id = 5, Name = "Changed", ExpenseCategoryTypeId = 0 });

        Assert.Equal(Prefix + "TypeLocked", ErrorText(result));
        f.Service.Verify(s => s.UpdateExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
    }

    [Fact]
    public async Task Update_SameType_UpdatesNameAndLimitOnlyAndKeepsTypeAndStores()
    {
        var f = new Fixture(Stored(ExpenseCategoryType.Income));

        var result = await f.Controller.ExpenseCategoryUpdate(new ExpenseCategoryModel
        {
            Id = 5,
            Name = "  Renamed ",
            PerMonthLimit = 0m,
            ExpenseCategoryTypeId = (int)ExpenseCategoryType.Income,
            LimitedToStores = false //must be ignored
        });

        Assert.IsType<NullJsonResult>(result);
        f.Service.Verify(s => s.UpdateExpenseCategoryAsync(f.Stored), Times.Once);
        Assert.Equal("Renamed", f.Stored.Name);
        Assert.Equal(0m, f.Stored.PerMonthLimit);
        Assert.Equal(ExpenseCategoryType.Income, f.Stored.ExpenseCategoryType);
        Assert.True(f.Stored.LimitedToStores);
    }

    [Fact]
    public async Task Update_ClearingTheLimit_Succeeds()
    {
        var f = new Fixture(Stored());

        var result = await f.Controller.ExpenseCategoryUpdate(new ExpenseCategoryModel { Id = 5, Name = "Original", PerMonthLimit = null, ExpenseCategoryTypeId = 1 });

        Assert.IsType<NullJsonResult>(result);
        Assert.Null(f.Stored.PerMonthLimit);
    }

    [Fact]
    public async Task Update_InvalidName_ReturnsFieldErrorAndLeavesEntityUntouched()
    {
        var f = new Fixture(Stored());

        var result = await f.Controller.ExpenseCategoryUpdate(new ExpenseCategoryModel
        {
            Id = 5,
            Name = new string('a', NopExpensesDefaults.NameMaxLength + 1),
            PerMonthLimit = 5m,
            ExpenseCategoryTypeId = 1
        });

        Assert.Equal(Prefix + "NameTooLong", FieldErrors(result)[nameof(ExpenseCategory.Name)]);
        f.Service.Verify(s => s.UpdateExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
        Assert.Equal("Original", f.Stored.Name);
        Assert.Equal(100m, f.Stored.PerMonthLimit);
    }

    [Fact]
    public async Task Update_NonNumericLimitModelStateError_MapsToLimitFieldErrorAndSavesNothing()
    {
        var f = new Fixture(Stored());
        f.Controller.ModelState.AddModelError(nameof(ExpenseCategoryModel.PerMonthLimit), "The value 'abc' is not valid.");

        var result = await f.Controller.ExpenseCategoryUpdate(new ExpenseCategoryModel { Id = 5, Name = "Original", PerMonthLimit = null, ExpenseCategoryTypeId = 1 });

        Assert.Equal(Prefix + "PerMonthLimitInvalid", FieldErrors(result)[nameof(ExpenseCategory.PerMonthLimit)]);
        f.Service.Verify(s => s.UpdateExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
        Assert.Equal(100m, f.Stored.PerMonthLimit);
    }

    [Fact]
    public async Task Update_NegativeLimit_ReturnsLimitFieldError()
    {
        var f = new Fixture(Stored());

        var result = await f.Controller.ExpenseCategoryUpdate(new ExpenseCategoryModel { Id = 5, Name = "Original", PerMonthLimit = -5m, ExpenseCategoryTypeId = 1 });

        Assert.Equal(Prefix + "PerMonthLimitInvalid", FieldErrors(result)[nameof(ExpenseCategory.PerMonthLimit)]);
        f.Service.Verify(s => s.UpdateExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
    }

    #endregion

    #region ModelState first (BUG-009)

    [Theory]
    [InlineData(nameof(ExpenseCategoryModel.ExpenseCategoryTypeId), "TypeRequired")]
    [InlineData(nameof(ExpenseCategoryModel.Name), "NameRequired")]
    [InlineData(nameof(ExpenseCategoryModel.PerMonthLimit), "PerMonthLimitInvalid")]
    public async Task Insert_BindingFailureOnAnyPostedField_ReturnsThatFieldsLocalizedErrorBeforeAnythingElse(string key, string messageKey)
    {
        var f = new Fixture();
        f.Controller.ModelState.AddModelError(key, "raw framework text");

        //the bound values are otherwise valid, so only the ModelState check can reject this
        var result = await f.Controller.ExpenseCategoryInsert(new ExpenseCategoryModel { Name = "A", ExpenseCategoryTypeId = 1 });

        Assert.Equal(Prefix + messageKey, FieldErrors(result)[key]);
        f.Service.Verify(s => s.InsertExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
    }

    [Fact]
    public async Task Insert_BindingFailureOnAnotherKey_ReturnsTheGenericLocalizedError()
    {
        var f = new Fixture();
        f.Controller.ModelState.AddModelError(nameof(ExpenseCategoryModel.LimitedToStores), "The value 'x' is not valid.");

        var result = await f.Controller.ExpenseCategoryInsert(new ExpenseCategoryModel { Name = "A", ExpenseCategoryTypeId = 1 });

        Assert.Equal(Prefix + "InvalidRequest", ErrorText(result));
        f.Service.Verify(s => s.InsertExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
    }

    [Fact]
    public async Task Update_BindingFailureOnId_IsRejectedBeforeTheLookup()
    {
        var f = new Fixture(Stored());
        f.Controller.ModelState.AddModelError(nameof(ExpenseCategoryModel.Id), "The value 'abc' is not valid.");

        var result = await f.Controller.ExpenseCategoryUpdate(new ExpenseCategoryModel { Id = 0, Name = "Changed", ExpenseCategoryTypeId = 1 });

        Assert.Equal(Prefix + "InvalidRequest", ErrorText(result));
        f.Service.Verify(s => s.GetExpenseCategoryByIdAsync(It.IsAny<int>()), Times.Never);
        f.Service.Verify(s => s.UpdateExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
    }

    [Fact]
    public async Task Update_BindingFailureOnType_IsRejectedWithTheTypeErrorAndNothingIsSaved()
    {
        var f = new Fixture(Stored());
        f.Controller.ModelState.AddModelError(nameof(ExpenseCategoryModel.ExpenseCategoryTypeId), "The value 'abc' is not valid.");

        var result = await f.Controller.ExpenseCategoryUpdate(new ExpenseCategoryModel { Id = 5, Name = "Changed", ExpenseCategoryTypeId = 0 });

        Assert.Equal(Prefix + "TypeRequired", FieldErrors(result)[nameof(ExpenseCategoryModel.ExpenseCategoryTypeId)]);
        f.Service.Verify(s => s.UpdateExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
        Assert.Equal("Original", f.Stored.Name);
    }

    #endregion

    #region Limit range and precision (BUG-002)

    [Theory]
    [InlineData("100000000000000", "PerMonthLimitTooLarge")]
    [InlineData("1.23456", "PerMonthLimitTooManyDecimals")]
    public async Task Insert_LimitBeyondTheColumn_ReturnsFieldErrorAndNeverReachesTheDatabase(string limit, string messageKey)
    {
        var f = new Fixture();

        var result = await f.Controller.ExpenseCategoryInsert(new ExpenseCategoryModel
        {
            Name = "A",
            PerMonthLimit = decimal.Parse(limit, System.Globalization.CultureInfo.InvariantCulture),
            ExpenseCategoryTypeId = 1
        });

        Assert.Equal(Prefix + messageKey, FieldErrors(result)[nameof(ExpenseCategory.PerMonthLimit)]);
        f.Service.Verify(s => s.InsertExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
    }

    [Fact]
    public async Task Update_LimitBeyondTheColumn_ReturnsFieldErrorAndLeavesEntityUntouched()
    {
        var f = new Fixture(Stored());

        var result = await f.Controller.ExpenseCategoryUpdate(new ExpenseCategoryModel
        {
            Id = 5,
            Name = "Original",
            PerMonthLimit = 100000000000000m,
            ExpenseCategoryTypeId = 1
        });

        Assert.Equal(Prefix + "PerMonthLimitTooLarge", FieldErrors(result)[nameof(ExpenseCategory.PerMonthLimit)]);
        f.Service.Verify(s => s.UpdateExpenseCategoryAsync(It.IsAny<ExpenseCategory>()), Times.Never);
        Assert.Equal(100m, f.Stored.PerMonthLimit);
    }

    #endregion
}
