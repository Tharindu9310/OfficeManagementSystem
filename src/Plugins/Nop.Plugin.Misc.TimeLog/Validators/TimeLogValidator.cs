using FluentValidation;
using Nop.Plugin.Misc.TimeLog.Services;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using TimeLogEntity = Nop.Plugin.Misc.TimeLog.Domain.TimeLog;

namespace Nop.Plugin.Misc.TimeLog.Validators;

/// <summary>
/// Represents the single, centralized time log field validator.
/// Reused as-is by the insert, update, and bulk-submit code paths in
/// <see cref="Services.TimeLogService"/> so the field rules are defined exactly once.
/// </summary>
public class TimeLogValidator : BaseNopValidator<TimeLogEntity>
{
    public TimeLogValidator(ILocalizationService localizationService, IProjectService projectService)
    {
        RuleFor(timeLog => timeLog.ProjectId)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.TimeLog.Validation.ProjectRequired"));

        RuleFor(timeLog => timeLog.ProjectId)
            .MustAsync(async (projectId, _) =>
            {
                if (projectId <= 0)
                    return true; //already caught by the NotEmpty rule above - avoid a duplicate message

                var project = await projectService.GetProjectByIdAsync(projectId);

                return project is { Active: true };
            })
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.TimeLog.Validation.ProjectInvalidOrInactive"));

        RuleFor(timeLog => timeLog.Task)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.TimeLog.Validation.TaskRequired"));

        RuleFor(timeLog => timeLog.Time)
            .InclusiveBetween(0m, 24m)
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.TimeLog.Validation.TimeOutOfRange"));

        RuleFor(timeLog => timeLog.Date)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.TimeLog.Validation.DateRequired"));
    }
}
