using Nop.Core;
using Nop.Plugin.Misc.TimeLog.Domain;

namespace Nop.Plugin.Misc.TimeLog.Services;

/// <summary>
/// Represents a project service
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Gets a project by identifier
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the project
    /// </returns>
    Task<Project> GetProjectByIdAsync(int projectId);

    /// <summary>
    /// Gets all active projects (cached)
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of active projects
    /// </returns>
    [Obsolete("Superseded by GetProjectsAssignedToCustomerAsync for eligibility-aware, per-staff filtering; retained for compatibility.")]
    Task<IList<Project>> GetAllActiveProjectsAsync();

    /// <summary>
    /// Gets all projects, paged, optionally filtered by name and store
    /// </summary>
    /// <param name="name">Project name to search for (optional)</param>
    /// <param name="storeId">Store identifier for store-mapping filtering; 0 to load all</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the paged list of projects
    /// </returns>
    Task<IPagedList<Project>> GetAllProjectsAsync(string name = null, int storeId = 0,
        int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// Inserts a project
    /// </summary>
    /// <param name="project">Project</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertProjectAsync(Project project);

    /// <summary>
    /// Updates a project
    /// </summary>
    /// <param name="project">Project</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateProjectAsync(Project project);

    /// <summary>
    /// Deletes a project, also removing any <see cref="ProjectStaffMapping"/> rows for it in the
    /// same operation. Callers are responsible for checking <see cref="HasTimeLogRecordsAsync"/>
    /// first (per the P2-14 delete-guard design, the hard-delete-vs-auto-Retired decision lives at
    /// the controller layer, not duplicated here)
    /// </summary>
    /// <param name="project">Project</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteProjectAsync(Project project);

    /// <summary>
    /// Gets the projects assigned to a customer (staff member), optionally restricted to only
    /// those currently eligible for new time-log entries (see
    /// <see cref="NopTimeLogDefaults.TimeLoggableProjectStatuses"/>) and/or a specific store
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="eligibleForTimeLoggingOnly">
    /// When true, only projects whose <see cref="Project.Status"/> is in
    /// <see cref="NopTimeLogDefaults.TimeLoggableProjectStatuses"/> are returned
    /// </param>
    /// <param name="storeId">Store identifier for store-mapping filtering; 0 to ignore store mapping</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of assigned projects
    /// </returns>
    Task<IList<Project>> GetProjectsAssignedToCustomerAsync(int customerId,
        bool eligibleForTimeLoggingOnly = false, int storeId = 0);

    /// <summary>
    /// Gets the identifiers of customers (staff members) currently assigned to a project
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of assigned customer identifiers
    /// </returns>
    Task<IList<int>> GetAssignedCustomerIdsAsync(int projectId);

    /// <summary>
    /// Saves the staff assignment for a project, synchronizing <see cref="ProjectStaffMapping"/> rows
    /// to exactly match the submitted set of customer identifiers - inserting new mappings and
    /// removing deselected ones (not append-only). An empty <paramref name="customerIds"/> list is a
    /// valid input and results in zero assigned staff
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <param name="customerIds">The full desired set of assigned customer identifiers</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task SaveStaffAssignmentsAsync(int projectId, IList<int> customerIds);

    /// <summary>
    /// Gets the number of staff members currently assigned to a project
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the assigned staff count
    /// </returns>
    Task<int> GetAssignedStaffCountAsync(int projectId);

    /// <summary>
    /// Gets a value indicating whether a project has any time log records against it, regardless of
    /// their <c>Draft</c>/<c>Submitted</c> status. Used by the delete-guard (Story P2-14) to decide
    /// whether a project may be hard-deleted
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains true if the project has at least one time log record; otherwise, false
    /// </returns>
    Task<bool> HasTimeLogRecordsAsync(int projectId);

    /// <summary>
    /// Sets a project's status and persists the change (also stamps <see cref="Project.UpdatedOnUtc"/>).
    /// Used by the delete-guard's auto-Retired transition (Story P2-14) so the status-only update
    /// stays a single service-layer operation rather than inline controller mutation
    /// </summary>
    /// <param name="project">Project</param>
    /// <param name="status">The new status</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task SetProjectStatusAsync(Project project, ProjectStatus status);
}
