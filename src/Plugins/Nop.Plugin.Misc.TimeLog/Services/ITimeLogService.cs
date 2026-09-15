using Nop.Core;
using Nop.Plugin.Misc.TimeLog.Domain;
using TimeLogEntity = Nop.Plugin.Misc.TimeLog.Domain.TimeLog;

namespace Nop.Plugin.Misc.TimeLog.Services;

/// <summary>
/// Represents a time log service
/// </summary>
public interface ITimeLogService
{
    /// <summary>
    /// Gets the current customer's own time log entries (row-owner scoped), paged and filtered
    /// </summary>
    /// <param name="customerId">Owning customer identifier - filter is applied in the repository query</param>
    /// <param name="dateFrom">Date range - from (inclusive)</param>
    /// <param name="dateTo">Date range - to (inclusive)</param>
    /// <param name="status">Status filter</param>
    /// <param name="projectId">Project filter</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size - clamped server-side to <see cref="NopTimeLogDefaults.MaxPageSize"/></param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the paged list of the customer's own time log entries
    /// </returns>
    Task<IPagedList<TimeLogEntity>> GetOwnTimeLogsAsync(int customerId,
        DateTime? dateFrom = null, DateTime? dateTo = null,
        TimeLogStatus? status = null, int? projectId = null,
        int pageIndex = 0, int pageSize = NopTimeLogDefaults.MaxPageSize);

    /// <summary>
    /// Gets all time log entries across staff (manager oversight view), paged and filtered
    /// </summary>
    /// <param name="customerId">Optional customer (staff) filter</param>
    /// <param name="dateFrom">Date range - from (inclusive)</param>
    /// <param name="dateTo">Date range - to (inclusive)</param>
    /// <param name="status">Status filter</param>
    /// <param name="projectId">Project filter</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size - clamped server-side to <see cref="NopTimeLogDefaults.MaxPageSize"/></param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the paged list of time log entries
    /// </returns>
    Task<IPagedList<TimeLogEntity>> GetAllTimeLogsAsync(
        int? customerId = null, DateTime? dateFrom = null, DateTime? dateTo = null,
        TimeLogStatus? status = null, int? projectId = null,
        int pageIndex = 0, int pageSize = NopTimeLogDefaults.MaxPageSize);

    /// <summary>
    /// Gets a time log entry by identifier, scoped to its owner
    /// </summary>
    /// <param name="timeLogId">Time log identifier</param>
    /// <param name="customerId">Owning customer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the time log entry, or null if not found OR not owned by <paramref name="customerId"/>
    /// (deliberately indistinguishable to avoid an existence-leak to a non-owning caller)
    /// </returns>
    Task<TimeLogEntity> GetOwnTimeLogByIdAsync(int timeLogId, int customerId);

    /// <summary>
    /// Gets a time log entry by identifier, unrestricted
    /// </summary>
    /// <remarks>
    /// For manager-oversight, permission-gated (<c>ManageTimeLogAll</c>) controller use only -
    /// callers MUST NOT expose this to a non-manager-permission caller.
    /// </remarks>
    /// <param name="timeLogId">Time log identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the time log entry
    /// </returns>
    Task<TimeLogEntity> GetTimeLogByIdAsync(int timeLogId);

    /// <summary>
    /// Inserts a time log entry
    /// </summary>
    /// <param name="timeLog">Time log entry</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertTimeLogAsync(TimeLogEntity timeLog);

    /// <summary>
    /// Updates a time log entry
    /// </summary>
    /// <remarks>
    /// Re-checks <see cref="TimeLogStatus.Draft"/> server-side before applying - throws
    /// <see cref="InvalidOperationException"/> if the record is no longer a Draft (never a silent no-op).
    /// </remarks>
    /// <param name="timeLog">Time log entry</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateTimeLogAsync(TimeLogEntity timeLog);

    /// <summary>
    /// Deletes a time log entry
    /// </summary>
    /// <remarks>
    /// Re-checks <see cref="TimeLogStatus.Draft"/> server-side before applying - throws
    /// <see cref="InvalidOperationException"/> if the record is no longer a Draft (never a silent no-op).
    /// </remarks>
    /// <param name="timeLog">Time log entry</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteTimeLogAsync(TimeLogEntity timeLog);

    /// <summary>
    /// Submits a batch of the customer's own Draft time log entries
    /// </summary>
    /// <remarks>
    /// For each id: fetches owner-scoped (silently records a failure for a missing/not-owned id,
    /// never throws for an individual bad row), re-runs the full field validator, re-checks the
    /// project is still Active, and on success sets Status = Submitted and refreshes UpdatedOnUtc.
    /// </remarks>
    /// <param name="timeLogIds">Time log identifiers to submit</param>
    /// <param name="customerId">Owning customer identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the per-id outcome list
    /// </returns>
    Task<IList<TimeLogSubmitResult>> SubmitTimeLogsAsync(IList<int> timeLogIds, int customerId);
}

