using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core;
using Nop.Core.Domain.Stores;

namespace Nop.Plugin.Misc.TimeLog.Domain;

/// <summary>
/// Represents a project that time can be logged against
/// </summary>
public class Project : BaseEntity, IStoreMappingSupported
{
    /// <summary>
    /// Gets or sets the project name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the project is active
    /// (Phase 1 field - retained for backward compatibility. Operationally superseded by
    /// <see cref="Status"/> eligibility (NotStarted/Inprogress) for time-logging purposes -
    /// see <see cref="NopTimeLogDefaults.TimeLoggableProjectStatuses"/>. Not removed so that
    /// existing Phase 1 callers of <c>GetAllActiveProjectsAsync</c> keep compiling.)
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the project start date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the project end date (optional; when set, must be greater than or equal to
    /// <see cref="StartDate"/>)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the project description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the project status identifier (backing field for <see cref="Status"/>)
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the project status
    /// </summary>
    [NotMapped]
    public ProjectStatus Status
    {
        get => (ProjectStatus)StatusId;
        set => StatusId = (int)value;
    }

    /// <summary>
    /// Gets or sets the date and time (UTC) the project was created
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time (UTC) the project was last updated
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the project is limited/restricted to certain stores
    /// </summary>
    public bool LimitedToStores { get; set; }
}
