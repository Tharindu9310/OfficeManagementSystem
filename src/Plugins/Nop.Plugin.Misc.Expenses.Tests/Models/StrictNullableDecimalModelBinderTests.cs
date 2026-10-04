using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Primitives;
using Moq;
using Nop.Plugin.Misc.Expenses.Models.Admin;
using Nop.Plugin.Misc.Expenses.Models.Binding;
using Xunit;

namespace Nop.Plugin.Misc.Expenses.Tests.Models;

/// <summary>
/// Tests for the strict Per Month Limit binder (BUG-001): real posted strings under real cultures, which the
/// controller tests (mocked ModelState) cannot cover
/// </summary>
public class StrictNullableDecimalModelBinderTests
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

    private static async Task<(ModelBindingResult Result, ModelStateDictionary State)> BindAsync(string posted, CultureInfo culture)
    {
        var modelName = nameof(ExpenseCategoryModel.PerMonthLimit);
        var values = new Dictionary<string, StringValues>();

        if (posted != null)
            values[modelName] = posted;

        var valueProvider = new Mock<IValueProvider>();
        valueProvider.Setup(p => p.GetValue(modelName)).Returns(posted == null
            ? ValueProviderResult.None
            : new ValueProviderResult(values[modelName], culture));

        var messageProvider = new DefaultModelBindingMessageProvider();
        var metadata = new Mock<ModelMetadata>(ModelMetadataIdentity.ForType(typeof(decimal?)));
        metadata.Setup(m => m.ModelBindingMessageProvider).Returns(messageProvider);

        var state = new ModelStateDictionary();
        var context = new DefaultModelBindingContext
        {
            ModelName = modelName,
            ModelState = state,
            ValueProvider = valueProvider.Object,
            ModelMetadata = metadata.Object
        };

        await new StrictNullableDecimalModelBinder().BindModelAsync(context);

        return (context.Result, state);
    }

    [Theory]
    [InlineData("1.5", "en-US", 1.5)]
    [InlineData("1,5", "de-DE", 1.5)]
    [InlineData("1234", "en-US", 1234)]
    [InlineData("0", "en-US", 0)]
    [InlineData("  12.5  ", "en-US", 12.5)]
    [InlineData("1234,5678", "de-DE", 1234.5678)]
    public async Task Bind_PlainNumberInTheCulturesFormat_IsAccepted(string posted, string cultureName, double expected)
    {
        var (result, state) = await BindAsync(posted, CultureInfo.GetCultureInfo(cultureName));

        Assert.True(result.IsModelSet);
        Assert.Equal((decimal)expected, result.Model);
        Assert.Equal(0, state.ErrorCount); //(IsValid stays false until MVC validation runs)
    }

    [Theory]
    [InlineData("1,5", "en-US")] //group separator: the stock binder read this as 15
    [InlineData("1.5", "de-DE")] //group separator: the stock binder read this as 15
    [InlineData("1,000", "en-US")]
    [InlineData("1.000,50", "de-DE")]
    [InlineData("1 000", "en-US")]
    [InlineData("1e3", "en-US")]
    [InlineData("$5", "en-US")]
    [InlineData("abc", "en-US")]
    [InlineData("1.2.3", "en-US")]
    [InlineData("79228162514264337593543950336", "en-US")] //beyond decimal
    public async Task Bind_GroupSeparatorsAndAmbiguousInput_IsRejectedWithAModelStateError(string posted, string cultureName)
    {
        var (result, state) = await BindAsync(posted, CultureInfo.GetCultureInfo(cultureName));

        Assert.False(result.IsModelSet);
        Assert.True(state.ErrorCount > 0);
        Assert.True(state.ContainsKey(nameof(ExpenseCategoryModel.PerMonthLimit)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Bind_EmptyValue_BindsToNullWithoutError(string posted)
    {
        var (result, state) = await BindAsync(posted, English);

        Assert.True(result.IsModelSet);
        Assert.Null(result.Model);
        Assert.Equal(0, state.ErrorCount); //(IsValid stays false until MVC validation runs)
    }

    [Fact]
    public async Task Bind_NotPosted_LeavesThePropertyUnboundWithoutError()
    {
        var (result, state) = await BindAsync(null, English);

        Assert.False(result.IsModelSet);
        Assert.Equal(0, state.ErrorCount); //(IsValid stays false until MVC validation runs)
    }

    [Fact]
    public async Task Bind_NegativeNumber_ParsesSoTheValidatorGivesTheRangeError()
    {
        var (result, _) = await BindAsync("-5", English);

        Assert.True(result.IsModelSet);
        Assert.Equal(-5m, result.Model);
    }

    [Fact]
    public void TryParseStrict_UsesTheGivenCultureNotTheThreadCulture()
    {
        Assert.True(StrictNullableDecimalModelBinder.TryParseStrict("1,5", German, out var value));
        Assert.Equal(1.5m, value);
        Assert.False(StrictNullableDecimalModelBinder.TryParseStrict("1,5", English, out _));
    }

    [Fact]
    public void PerMonthLimit_IsBoundWithTheStrictBinder()
    {
        var attribute = typeof(ExpenseCategoryModel).GetProperty(nameof(ExpenseCategoryModel.PerMonthLimit))!
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.ModelBinderAttribute), false)
            .Cast<Microsoft.AspNetCore.Mvc.ModelBinderAttribute>().SingleOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal(typeof(StrictNullableDecimalModelBinder), attribute.BinderType);
    }
}
