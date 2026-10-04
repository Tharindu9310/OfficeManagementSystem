using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Expenses.Models.Binding;

/// <summary>
/// Binds a nullable decimal strictly under the request culture. Unlike the stock decimal binder, group
/// (thousands) separators, exponents and currency symbols are rejected rather than silently ignored, so
/// that "1,5" (en) or "1.5" (de) can never be read as 15. An empty value binds to null; anything that
/// is not a plain number in the culture's own format is a model state error, which the controller maps
/// to the localized field error.
/// </summary>
public class StrictNullableDecimalModelBinder : IModelBinder
{
    /// <summary>
    /// The only styles accepted: optional sign and a decimal point. Deliberately no AllowThousands.
    /// </summary>
    private const NumberStyles Styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    /// <summary>
    /// Attempts to parse a posted value strictly
    /// </summary>
    /// <param name="text">Posted text</param>
    /// <param name="culture">Culture whose number format applies</param>
    /// <param name="value">Parsed value</param>
    /// <returns>True when the text is a plain number in the culture's format</returns>
    public static bool TryParseStrict(string text, IFormatProvider culture, out decimal value)
    {
        value = 0m;

        return !string.IsNullOrWhiteSpace(text)
            && decimal.TryParse(text.Trim(), Styles, culture ?? CultureInfo.CurrentCulture, out value);
    }

    /// <summary>
    /// Binds the model
    /// </summary>
    /// <param name="bindingContext">Binding context</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        //not posted: leave the property unbound (null)
        if (valueResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

        var text = valueResult.FirstValue?.Trim();

        if (string.IsNullOrEmpty(text))
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        if (TryParseStrict(text, valueResult.Culture, out var value))
        {
            bindingContext.Result = ModelBindingResult.Success(value);
            return Task.CompletedTask;
        }

        //the controller replaces this framework text with the localized PerMonthLimitInvalid message
        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
            bindingContext.ModelMetadata.ModelBindingMessageProvider.AttemptedValueIsInvalidAccessor(text,
                bindingContext.ModelMetadata.DisplayName ?? bindingContext.ModelName));

        return Task.CompletedTask;
    }
}
