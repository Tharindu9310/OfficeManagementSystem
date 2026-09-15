using FluentValidation;
using Nop.Plugin.Misc.TimeLog.Domain;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.TimeLog.Validators;

/// <summary>
/// Represents the project field validator (TT-032/T-023). Mirrors
/// <see cref="TimeLogValidator"/>'s shape/conventions - a single, centralized validator reused by
/// both the Create and Edit POST actions in <c>ProjectController</c> so field rules are defined
/// exactly once and enforced server-side regardless of client-side state (CLAUDE.md validation
/// standard), including against a tampered out-of-range Status integer.
/// </summary>
public class ProjectValidator : BaseNopValidator<Project>
{
    public ProjectValidator(ILocalizationService localizationService)
    {
        RuleFor(project => project.Name)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.TimeLog.Project.Validation.NameRequired"));

        RuleFor(project => project.StartDate)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.TimeLog.Project.Validation.StartDateRequired"));

        RuleFor(project => project.EndDate)
            .Must((project, endDate) => !endDate.HasValue || endDate.Value.Date >= project.StartDate.Date)
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.TimeLog.Project.Validation.EndDateBeforeStartDate"));

        RuleFor(project => project.StatusId)
            .Must(statusId => Enum.IsDefined(typeof(ProjectStatus), statusId))
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.TimeLog.Project.Validation.StatusInvalid"));
    }
}
