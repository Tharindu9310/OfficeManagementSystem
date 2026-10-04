using Moq;
using Nop.Plugin.Misc.Expenses.Domain;
using Nop.Plugin.Misc.Expenses.Validators;
using Nop.Services.Localization;
using Xunit;

namespace Nop.Plugin.Misc.Expenses.Tests.Validators;

/// <summary>
/// Unit tests for <see cref="ExpenseCategoryValidator"/> (AC4.3, AC4.7 to AC4.9, AC4.11, AC4.12)
/// </summary>
public class ExpenseCategoryValidatorTests
{
    private const string Prefix = "Admin.Expenses.ExpenseCategory.Validation.";

    private static ExpenseCategoryValidator CreateValidator()
    {
        var localization = new Mock<ILocalizationService>();
        localization.Setup(l => l.GetResourceAsync(It.IsAny<string>())).ReturnsAsync((string key) => key);

        return new ExpenseCategoryValidator(localization.Object);
    }

    private static ExpenseCategory CreateValid(ExpenseCategoryType type = ExpenseCategoryType.Expense) => new()
    {
        Name = "Office supplies",
        PerMonthLimit = null,
        ExpenseCategoryType = type
    };

    [Fact]
    public async Task Validate_AllFieldsValid_Passes()
    {
        var result = await CreateValidator().ValidateAsync(CreateValid());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_BlankName_FailsNameRequired(string name)
    {
        var category = CreateValid();
        category.Name = name;

        var result = await CreateValidator().ValidateAsync(category);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ExpenseCategory.Name) && e.ErrorMessage == Prefix + "NameRequired");
    }

    [Fact]
    public async Task Validate_Name201Characters_FailsNameTooLong()
    {
        var category = CreateValid();
        category.Name = new string('a', NopExpensesDefaults.NameMaxLength + 1);

        var result = await CreateValidator().ValidateAsync(category);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ExpenseCategory.Name) && e.ErrorMessage == Prefix + "NameTooLong");
    }

    [Fact]
    public async Task Validate_Name200Characters_Passes()
    {
        var category = CreateValid();
        category.Name = new string('a', NopExpensesDefaults.NameMaxLength);

        var result = await CreateValidator().ValidateAsync(category);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_DuplicateName_IsAccepted()
    {
        //there is no uniqueness rule: the validator has no access to existing names, and the same
        //name validates the same way every time
        var validator = CreateValidator();

        Assert.True((await validator.ValidateAsync(CreateValid())).IsValid);
        Assert.True((await validator.ValidateAsync(CreateValid())).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    [InlineData(99)]
    public async Task Validate_UnsetOrUndefinedType_FailsTypeRequired(int typeId)
    {
        var category = CreateValid();
        category.ExpenseCategoryTypeId = typeId;

        var result = await CreateValidator().ValidateAsync(category);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ExpenseCategory.ExpenseCategoryTypeId) && e.ErrorMessage == Prefix + "TypeRequired");
    }

    [Theory]
    [InlineData(ExpenseCategoryType.Expense)]
    [InlineData(ExpenseCategoryType.Income)]
    public async Task Validate_DefinedType_Passes(ExpenseCategoryType type)
    {
        var result = await CreateValidator().ValidateAsync(CreateValid(type));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(ExpenseCategoryType.Expense)]
    [InlineData(ExpenseCategoryType.Income)]
    public async Task Validate_NullZeroAndPositiveLimit_Passes(ExpenseCategoryType type)
    {
        var validator = CreateValidator();

        foreach (decimal? limit in new decimal?[] { null, 0m, 0.01m, 1234.5678m })
        {
            var category = CreateValid(type);
            category.PerMonthLimit = limit;

            Assert.True((await validator.ValidateAsync(category)).IsValid, $"{type} limit {limit}");
        }
    }

    [Theory]
    [InlineData(ExpenseCategoryType.Expense)]
    [InlineData(ExpenseCategoryType.Income)]
    public async Task Validate_NegativeLimit_FailsPerMonthLimitInvalid(ExpenseCategoryType type)
    {
        var category = CreateValid(type);
        category.PerMonthLimit = -0.01m;

        var result = await CreateValidator().ValidateAsync(category);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ExpenseCategory.PerMonthLimit) && e.ErrorMessage == Prefix + "PerMonthLimitInvalid");
    }

    [Fact]
    public async Task Validate_LimitAtTheColumnMaximum_Passes()
    {
        var category = CreateValid();
        category.PerMonthLimit = NopExpensesDefaults.PerMonthLimitMaxValue; //99999999999999.9999 = decimal(18,4) maximum

        Assert.True((await CreateValidator().ValidateAsync(category)).IsValid);
    }

    [Theory]
    [InlineData("100000000000000")]
    [InlineData("99999999999999.99995")]
    [InlineData("79228162514264337593543950335")]
    public async Task Validate_LimitAboveTheColumnMaximum_FailsPerMonthLimitTooLarge(string limit)
    {
        var category = CreateValid();
        category.PerMonthLimit = decimal.Parse(limit, System.Globalization.CultureInfo.InvariantCulture);

        var result = await CreateValidator().ValidateAsync(category);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ExpenseCategory.PerMonthLimit) && e.ErrorMessage == Prefix + "PerMonthLimitTooLarge");
    }

    [Theory]
    [InlineData("1.2345")]
    [InlineData("1.50000")] //trailing zeros are not extra precision
    [InlineData("0.0001")]
    public async Task Validate_LimitWithAtMostFourSignificantDecimals_Passes(string limit)
    {
        var category = CreateValid();
        category.PerMonthLimit = decimal.Parse(limit, System.Globalization.CultureInfo.InvariantCulture);

        Assert.True((await CreateValidator().ValidateAsync(category)).IsValid);
    }

    [Theory]
    [InlineData("1.23456")]
    [InlineData("0.00001")]
    [InlineData("5.00001")]
    public async Task Validate_LimitWithMoreThanFourDecimals_FailsPerMonthLimitTooManyDecimals(string limit)
    {
        var category = CreateValid();
        category.PerMonthLimit = decimal.Parse(limit, System.Globalization.CultureInfo.InvariantCulture);

        var result = await CreateValidator().ValidateAsync(category);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ExpenseCategory.PerMonthLimit) && e.ErrorMessage == Prefix + "PerMonthLimitTooManyDecimals");
    }

    [Fact]
    public async Task Validate_LimitMessages_FillTheirPlaceholdersFromTheDefaults()
    {
        var localization = new Mock<ILocalizationService>();
        localization.Setup(l => l.GetResourceAsync(It.IsAny<string>())).ReturnsAsync((string key) => key + "|{0}");
        var validator = new ExpenseCategoryValidator(localization.Object);
        var category = CreateValid();
        category.Name = new string('a', NopExpensesDefaults.NameMaxLength + 1);
        category.PerMonthLimit = 1.23456m;

        var first = await validator.ValidateAsync(category);
        category.Name = "ok";
        category.PerMonthLimit = 100000000000000m;
        var second = await validator.ValidateAsync(category);

        Assert.Contains(first.Errors, e => e.ErrorMessage == Prefix + "NameTooLong|200");
        Assert.Contains(first.Errors, e => e.ErrorMessage == Prefix + "PerMonthLimitTooManyDecimals|4");
        Assert.Contains(second.Errors, e => e.ErrorMessage.StartsWith(Prefix + "PerMonthLimitTooLarge|99999999999999"));
    }
}
