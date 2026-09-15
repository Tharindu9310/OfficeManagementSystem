using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TimeLog.Domain;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.TimeLog.Services;

/// <summary>
/// Represents a project service
/// </summary>
public class ProjectService : IProjectService
{
    #region Fields

    protected readonly IRepository<Project> _projectRepository;
    protected readonly IRepository<ProjectStaffMapping> _projectStaffMappingRepository;
    protected readonly IRepository<Domain.TimeLog> _timeLogRepository;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IStoreMappingService _storeMappingService;

    #endregion

    #region Ctor

    public ProjectService(IRepository<Project> projectRepository,
        IRepository<ProjectStaffMapping> projectStaffMappingRepository,
        IRepository<Domain.TimeLog> timeLogRepository,
        IStaticCacheManager staticCacheManager,
        IStoreMappingService storeMappingService)
    {
        _projectRepository = projectRepository;
        _projectStaffMappingRepository = projectStaffMappingRepository;
        _timeLogRepository = timeLogRepository;
        _staticCacheManager = staticCacheManager;
        _storeMappingService = storeMappingService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a project by identifier
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the project
    /// </returns>
    public virtual async Task<Project> GetProjectByIdAsync(int projectId)
    {
        return await _projectRepository.GetByIdAsync(projectId);
    }

    /// <summary>
    /// Gets all active projects (cached)
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of active projects
    /// </returns>
    [Obsolete("Superseded by GetProjectsAssignedToCustomerAsync for eligibility-aware, per-staff filtering; retained for compatibility.")]
    public virtual async Task<IList<Project>> GetAllActiveProjectsAsync()
    {
        return await _staticCacheManager.GetAsync(NopTimeLogDefaults.ActiveProjectsCacheKey, async () =>
            await _projectRepository.GetAllAsync(query =>
            {
                query = query.Where(p => p.Active).OrderBy(p => p.Name);

                return query;
            }));
    }

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
    public virtual async Task<IPagedList<Project>> GetAllProjectsAsync(string name = null, int storeId = 0,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _projectRepository.Table;

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));

        if (storeId > 0)
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

        query = query.OrderBy(p => p.Name);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Inserts a project
    /// </summary>
    /// <param name="project">Project</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertProjectAsync(Project project)
    {
        await _projectRepository.InsertAsync(project);
        await _staticCacheManager.RemoveByPrefixAsync(NopTimeLogDefaults.ProjectsPatternCacheKey);
    }

    /// <summary>
    /// Updates a project
    /// </summary>
    /// <param name="project">Project</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateProjectAsync(Project project)
    {
        await _projectRepository.UpdateAsync(project);
        await _staticCacheManager.RemoveByPrefixAsync(NopTimeLogDefaults.ProjectsPatternCacheKey);
    }

    /// <summary>
    /// Deletes a project, also removing any <see cref="ProjectStaffMapping"/> rows for it in the
    /// same operation. Callers are responsible for checking <see cref="HasTimeLogRecordsAsync"/>
    /// first (per the P2-14 delete-guard design, the hard-delete-vs-auto-Retired decision lives at
    /// the controller layer, not duplicated here)
    /// </summary>
    /// <param name="project">Project</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteProjectAsync(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        var mappings = await _projectStaffMappingRepository.GetAllAsync(query =>
            query.Where(m => m.ProjectId == project.Id));

        if (mappings.Any())
            await _projectStaffMappingRepository.DeleteAsync(mappings);

        await _projectRepository.DeleteAsync(project);
        await _staticCacheManager.RemoveByPrefixAsync(NopTimeLogDefaults.ProjectsPatternCacheKey);
    }

    /// <summary>
    /// Gets the projects assigned to a customer (staff member), optionally restricted to only
    /// those currently eligible for new time-log entries and/or a specific store
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
    public virtual async Task<IList<Project>> GetProjectsAssignedToCustomerAsync(int customerId,
        bool eligibleForTimeLoggingOnly = false, int storeId = 0)
    {
        var query =
            from project in _projectRepository.Table
            join mapping in _projectStaffMappingRepository.Table on project.Id equals mapping.ProjectId
            where mapping.CustomerId == customerId
            select project;

        query = query.Distinct();

        if (eligibleForTimeLoggingOnly)
        {
            var eligibleStatusIds = NopTimeLogDefaults.TimeLoggableProjectStatuses.Select(s => (int)s).ToArray();
            query = query.Where(p => eligibleStatusIds.Contains(p.StatusId));
        }

        if (storeId > 0)
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

        query = query.OrderBy(p => p.Name);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets the identifiers of customers (staff members) currently assigned to a project
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of assigned customer identifiers
    /// </returns>
    public virtual async Task<IList<int>> GetAssignedCustomerIdsAsync(int projectId)
    {
        var query = _projectStaffMappingRepository.Table
            .Where(m => m.ProjectId == projectId)
            .Select(m => m.CustomerId);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Saves the staff assignment for a project, synchronizing <see cref="ProjectStaffMapping"/> rows
    /// to exactly match the submitted set of customer identifiers - inserting new mappings and
    /// removing deselected ones (not append-only). An empty <paramref name="customerIds"/> list is a
    /// valid input and results in zero assigned staff
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <param name="customerIds">The full desired set of assigned customer identifiers</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task SaveStaffAssignmentsAsync(int projectId, IList<int> customerIds)
    {
        customerIds ??= new List<int>();

        var existingMappings = await _projectStaffMappingRepository.GetAllAsync(query =>
            query.Where(m => m.ProjectId == projectId));

        var mappingsToRemove = existingMappings
            .Where(m => !customerIds.Contains(m.CustomerId))
            .ToList();

        var existingCustomerIds = existingMappings.Select(m => m.CustomerId).ToHashSet();
        var mappingsToAdd = customerIds
            .Where(customerId => !existingCustomerIds.Contains(customerId))
            .Distinct()
            .Select(customerId => new ProjectStaffMapping { ProjectId = projectId, CustomerId = customerId })
            .ToList();

        if (mappingsToRemove.Any())
            await _projectStaffMappingRepository.DeleteAsync(mappingsToRemove);

        if (mappingsToAdd.Any())
            await _projectStaffMappingRepository.InsertAsync(mappingsToAdd);
    }

    /// <summary>
    /// Gets the number of staff members currently assigned to a project
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the assigned staff count
    /// </returns>
    public virtual async Task<int> GetAssignedStaffCountAsync(int projectId)
    {
        return await _projectStaffMappingRepository.Table
            .Where(m => m.ProjectId == projectId)
            .CountAsync();
    }

    /// <summary>
    /// Gets a value indicating whether a project has any time log records against it, regardless of
    /// their Draft/Submitted status
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains true if the project has at least one time log record; otherwise, false
    /// </returns>
    public virtual async Task<bool> HasTimeLogRecordsAsync(int projectId)
    {
        return await _timeLogRepository.Table.AnyAsync(t => t.ProjectId == projectId);
    }

    /// <summary>
    /// Sets a project's status and persists the change (also stamps <see cref="Project.UpdatedOnUtc"/>).
    /// Used by the delete-guard's auto-Retired transition (Story P2-14)
    /// </summary>
    /// <param name="project">Project</param>
    /// <param name="status">The new status</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task SetProjectStatusAsync(Project project, ProjectStatus status)
    {
        ArgumentNullException.ThrowIfNull(project);

        project.Status = status;
        project.UpdatedOnUtc = DateTime.UtcNow;

        await UpdateProjectAsync(project);
    }

    #endregion
}
