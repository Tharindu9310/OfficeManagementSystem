using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.TimeLog.Domain;
using Nop.Plugin.Misc.TimeLog.Validators;
using Nop.Services.Localization;
using TimeLogEntity = Nop.Plugin.Misc.TimeLog.Domain.TimeLog;

namespace Nop.Plugin.Misc.TimeLog.Services;

/// <summary>
/// Represents a time log service
/// </summary>
public class TimeLogService : ITimeLogService
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly IProjectService _projectService;
    protected readonly IRepository<TimeLogEntity> _timeLogRepository;
    protected readonly TimeLogValidator _timeLogValidator;

    #endregion

    #region Ctor

    public TimeLogService(ILocalizationService localizationService,
        IProjectService projectService,
        IRepository<TimeLogEntity> timeLogRepository,
        TimeLogValidator timeLogValidator)
    {
        _localizationService = localizationService;
        _projectService = projectService;
        _timeLogRepository = timeLogRepository;
        _timeLogValidator = timeLogValidator;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Builds the shared filter predicate (date range/status/project) applied on top of the repository query.
    /// Ownership (customerId) filtering, when required, is applied by each caller directly in the same query -
    /// never as a separate fetch-then-filter step.
    /// </summary>
    private static IQueryable<TimeLogEntity> ApplyCommonFilter(IQueryable<TimeLogEntity> query,
        DateTime? dateFrom, DateTime? dateTo, TimeLogStatus? status, int? projectId)
    {
        if (dateFrom.HasValue)
            query = query.Where(t => t.Date >= dateFrom.Value.Date);

        if (dateTo.HasValue)
            query = query.Where(t => t.Date <= dateTo.Value.Date);

        if (status.HasValue)
            query = query.Where(t => t.StatusId == (int)status.Value);

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        return query.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id);
    }

    /// <summary>
    /// Clamps the requested page size to the plugin-wide maximum
    /// </summary>
    private static int ClampPageSize(int pageSize)
    {
        return pageSize <= 0 ? NopTimeLogDefaults.MaxPageSize : Math.Min(pageSize, NopTimeLogDefaults.MaxPageSize);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the current customer's own time log entries (row-owner scoped), paged and filtered
    /// </summary>
    public virtual async Task<IPagedList<TimeLogEntity>> GetOwnTimeLogsAsync(int customerId,
        DateTime? dateFrom = null, DateTime? dateTo = null,
        TimeLogStatus? status = null, int? projectId = null,
        int pageIndex = 0, int pageSize = NopTimeLogDefaults.MaxPageSize)
    {
        return await _timeLogRepository.GetAllPagedAsync(query =>
        {
            //ownership filter MUST be part of the same query - never fetch-then-filter-in-memory
            query = query.Where(t => t.CustomerId == customerId);
            query = ApplyCommonFilter(query, dateFrom, dateTo, status, projectId);

            return query;
        }, pageIndex, ClampPageSize(pageSize));
    }

    /// <summary>
    /// Gets all time log entries across staff (manager oversight view), paged and filtered
    /// </summary>
    public virtual async Task<IPagedList<TimeLogEntity>> GetAllTimeLogsAsync(
        int? customerId = null, DateTime? dateFrom = null, DateTime? dateTo = null,
        TimeLogStatus? status = null, int? projectId = null,
        int pageIndex = 0, int pageSize = NopTimeLogDefaults.MaxPageSize)
    {
        return await _timeLogRepository.GetAllPagedAsync(query =>
        {
            if (customerId.HasValue)
                query = query.Where(t => t.CustomerId == customerId.Value);

            query = ApplyCommonFilter(query, dateFrom, dateTo, status, projectId);

            return query;
        }, pageIndex, ClampPageSize(pageSize));
    }

    /// <summary>
    /// Gets a time log entry by identifier, scoped to its owner.
    /// Returns null if not found OR not owned - the two cases are deliberately indistinguishable
    /// to the caller so a non-owning request cannot be used to probe for record existence.
    /// </summary>
    public virtual async Task<TimeLogEntity> GetOwnTimeLogByIdAsync(int timeLogId, int customerId)
    {
        if (timeLogId <= 0)
            return null;

        //ownership check is part of the query itself, not a post-fetch comparison
        return await _timeLogRepository.Table
            .FirstOrDefaultAsync(t => t.Id == timeLogId && t.CustomerId == customerId);
    }

    /// <summary>
    /// Gets a time log entry by identifier, unrestricted.
    /// For manager-oversight, permission-gated controller use only.
    /// </summary>
    public virtual async Task<TimeLogEntity> GetTimeLogByIdAsync(int timeLogId)
    {
        return await _timeLogRepository.GetByIdAsync(timeLogId);
    }

    /// <summary>
    /// Inserts a time log entry
    /// </summary>
    public virtual async Task InsertTimeLogAsync(TimeLogEntity timeLog)
    {
        ArgumentNullException.ThrowIfNull(timeLog);

        //defense in depth (BUG-001/AC-10): re-run the centralized field validator here too, so this
        //service method independently rejects an invalid/inactive Project, empty Task, or out-of-range
        //Time even if a future caller ever bypasses the controller-level check
        await ValidateOrThrowAsync(timeLog);

        await _timeLogRepository.InsertAsync(timeLog);
    }

    /// <summary>
    /// Updates a time log entry - re-checks Draft status and field validation server-side before applying
    /// </summary>
    public virtual async Task UpdateTimeLogAsync(TimeLogEntity timeLog)
    {
        ArgumentNullException.ThrowIfNull(timeLog);

        var current = await _timeLogRepository.GetByIdAsync(timeLog.Id);

        if (current == null || current.Status != TimeLogStatus.Draft)
            throw new InvalidOperationException(await _localizationService.GetResourceAsync("Admin.TimeLog.Validation.RecordNotEditable"));

        //defense in depth (BUG-001/AC-4.3/AC-10): never trust that the controller already validated -
        //re-run the same centralized validator used by SubmitTimeLogsAsync
        await ValidateOrThrowAsync(timeLog);

        await _timeLogRepository.UpdateAsync(timeLog);
    }

    /// <summary>
    /// Runs the centralized <see cref="TimeLogValidator"/> and throws <see cref="InvalidOperationException"/>
    /// with the joined field error messages when invalid - never silently persists an invalid entity.
    /// </summary>
    private async Task ValidateOrThrowAsync(TimeLogEntity timeLog)
    {
        var validationResult = await _timeLogValidator.ValidateAsync(timeLog);

        if (!validationResult.IsValid)
            throw new InvalidOperationException(string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage)));
    }

    /// <summary>
    /// Deletes a time log entry - re-checks Draft status server-side before applying
    /// </summary>
    public virtual async Task DeleteTimeLogAsync(TimeLogEntity timeLog)
    {
        ArgumentNullException.ThrowIfNull(timeLog);

        var current = await _timeLogRepository.GetByIdAsync(timeLog.Id);

        if (current == null || current.Status != TimeLogStatus.Draft)
            throw new InvalidOperationException(await _localizationService.GetResourceAsync("Admin.TimeLog.Validation.RecordNotEditable"));

        await _timeLogRepository.DeleteAsync(timeLog);
    }

    /// <summary>
    /// Submits a batch of the customer's own Draft time log entries.
    /// Never throws for an individual bad row - only aggregate per-id results are returned.
    /// </summary>
    public virtual async Task<IList<TimeLogSubmitResult>> SubmitTimeLogsAsync(IList<int> timeLogIds, int customerId)
    {
        var results = new List<TimeLogSubmitResult>();

        if (timeLogIds == null || !timeLogIds.Any())
            return results;

        foreach (var timeLogId in timeLogIds)
        {
            //owner-scoped fetch - a missing or not-owned id is recorded as a failure, never thrown
            var timeLog = await GetOwnTimeLogByIdAsync(timeLogId, customerId);

            if (timeLog == null)
            {
                results.Add(new TimeLogSubmitResult
                {
                    TimeLogId = timeLogId,
                    Success = false,
                    ErrorMessage = await _localizationService.GetResourceAsync("Admin.TimeLog.Validation.RecordNotFound")
                });

                continue;
            }

            if (timeLog.Status != TimeLogStatus.Draft)
            {
                results.Add(new TimeLogSubmitResult
                {
                    TimeLogId = timeLogId,
                    Success = false,
                    ErrorMessage = await _localizationService.GetResourceAsync("Admin.TimeLog.Submit.RecordNotDraft")
                });

                continue;
            }

            //re-run the full field validator - never trust client-side validation state
            var validationResult = await _timeLogValidator.ValidateAsync(timeLog);

            if (!validationResult.IsValid)
            {
                results.Add(new TimeLogSubmitResult
                {
                    TimeLogId = timeLogId,
                    Success = false,
                    ErrorMessage = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage))
                });

                continue;
            }

            //BUG-008 fix: eligibility re-check now uses the same shared eligibility source as every
            //other Phase 2 path (staff dropdown/filter, TimeLogInsert/TimeLogUpdate re-validation) -
            //the project's Status against NopTimeLogDefaults.TimeLoggableProjectStatuses - rather than
            //the legacy Project.Active flag, which is set unconditionally by ProjectController on every
            //save and is never synced from Status, so it does not reflect real eligibility
            var project = await _projectService.GetProjectByIdAsync(timeLog.ProjectId);

            if (project == null || !NopTimeLogDefaults.TimeLoggableProjectStatuses.Contains(project.Status))
            {
                results.Add(new TimeLogSubmitResult
                {
                    TimeLogId = timeLogId,
                    Success = false,
                    ErrorMessage = await _localizationService.GetResourceAsync("Admin.TimeLog.Submit.ProjectInactive")
                });

                continue;
            }

            timeLog.Status = TimeLogStatus.Submitted;
            timeLog.UpdatedOnUtc = DateTime.UtcNow;
            await _timeLogRepository.UpdateAsync(timeLog);

            results.Add(new TimeLogSubmitResult
            {
                TimeLogId = timeLogId,
                Success = true
            });
        }

        return results;
    }

    #endregion
}
