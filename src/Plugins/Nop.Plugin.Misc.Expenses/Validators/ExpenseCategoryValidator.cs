using System.Globalization;
using FluentValidation;
using Nop.Plugin.Misc.Expenses.Domain;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.Expenses.Validators;

/// <summary>
/// Represents the expense category validator. A single validator shared by the insert and update
/// actions so the rules are enforced identically for the UI and for direct requests. Duplicate names
/// are allowed, so there is deliberately no uniqueness rule.
/// </summary>
public class ExpenseCategoryValidator : BaseNopValidator<ExpenseCategory>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpenseCategoryValidator"/> class
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public ExpenseCategoryValidator(ILocalizationService localizationService)
    {
        const string prefix = "Admin.Expenses.ExpenseCategory.Validation.";

        RuleFor(category => category.Name)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync(prefix + "NameRequired"));

        RuleFor(category => category.Name)
            .MaximumLength(NopExpensesDefaults.NameMaxLength)
            .WithMessageAwait(FormatAsync(localizationService, prefix + "NameTooLong", NopExpensesDefaults.NameMaxLength));

        //the unset value (0) and any undefined integer are rejected
        RuleFor(category => category.ExpenseCategoryTypeId)
            .Must(typeId => Enum.IsDefined(typeof(ExpenseCategoryType), typeId))
            .WithMessageAwait(localizationService.GetResourceAsync(prefix + "TypeRequired"));

        //optional; when present it must be 0 or greater (both Expense and Income)
        RuleFor(category => category.PerMonthLimit)
            .GreaterThanOrEqualTo(0m)
            .When(category => category.PerMonthLimit.HasValue)
            .WithMessageAwait(localizationService.GetResourceAsync(prefix + "PerMonthLimitInvalid"));

        //the column is decimal(18,4): a larger value would overflow in the database and surface as an
        //unhandled error, so it is rejected here with a field error instead
        RuleFor(category => category.PerMonthLimit)
            .LessThanOrEqualTo(NopExpensesDefaults.PerMonthLimitMaxValue)
            .When(category => category.PerMonthLimit.HasValue && category.PerMonthLimit.Value >= 0m)
            .WithMessageAwait(FormatAsync(localizationService, prefix + "PerMonthLimitTooLarge",
                NopExpensesDefaults.PerMonthLimitMaxValue.ToString("0.####", CultureInfo.CurrentCulture)));

        //more decimals than the column keeps would be silently rounded, so the stored value would differ
        //from the entered one; trailing zeros do not count (1.50000 is exactly representable)
        RuleFor(category => category.PerMonthLimit)
            .Must(limit => Math.Round(limit.Value, NopExpensesDefaults.PerMonthLimitMaxScale) == limit.Value)
            .When(category => category.PerMonthLimit.HasValue)
            .WithMessageAwait(FormatAsync(localizationService, prefix + "PerMonthLimitTooManyDecimals",
                NopExpensesDefaults.PerMonthLimitMaxScale));
    }

    /// <summary>
    /// Gets a localized message and fills its {0} placeholder
    /// </summary>
    private static async Task<string> FormatAsync(ILocalizationService localizationService, string key, object argument)
    {
        return string.Format(CultureInfo.CurrentCulture, await localizationService.GetResourceAsync(key), argument);
    }
}
