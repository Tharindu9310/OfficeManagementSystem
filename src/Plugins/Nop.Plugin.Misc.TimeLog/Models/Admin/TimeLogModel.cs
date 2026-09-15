using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.TimeLog.Models.Admin;

/// <summary>
/// Represents a time log entry model (self-service grid row)
/// </summary>
public record TimeLogModel : BaseNopEntityModel
{
    /// <summary>
    /// Gets or sets the project identifier
    /// </summary>
    [NopResourceDisplayName("Admin.TimeLog.Fields.Project")]
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name (display-only)
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    /// Gets or sets the task
    /// </summary>
    [NopResourceDisplayName("Admin.TimeLog.Fields.Task")]
    public string Task { get; set; }

    /// <summary>
    /// Gets or sets the description
    /// </summary>
    [NopResourceDisplayName("Admin.TimeLog.Fields.Description")]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the date
    /// </summary>
    [NopResourceDisplayName("Admin.TimeLog.Fields.Date")]
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the number of hours logged, as an exact decimal.
    /// This is the field the server binds/validates - the HH:mm string (<see cref="TimeDisplay"/>)
    /// is a UI-layer-only concern and never reaches server-side validation directly.
    /// </summary>
    [NopResourceDisplayName("Admin.TimeLog.Fields.Time")]
    public decimal Time { get; set; }

    /// <summary>
    /// Gets or sets the HH:mm display string, computed from <see cref="Time"/> by reconstructing
    /// the exact minute value (round(hours * 60)) rather than formatting the decimal directly -
    /// see Domain.TimeLog.Time for the rounding rationale (non-terminating fractions e.g. 20 minutes).
    /// </summary>
    [NopResourceDisplayName("Admin.TimeLog.Fields.Time")]
    public string TimeDisplay { get; set; }

    /// <summary>
    /// Gets or sets the status identifier
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the localized status display text
    /// </summary>
    [NopResourceDisplayName("Admin.TimeLog.Fields.Status")]
    public string StatusName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this row may still be edited (server-supplied,
    /// true only when Status == Draft) - the grid must not infer editability client-side
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this row may still be deleted (server-supplied,
    /// true only when Status == Draft)
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this row is selectable for bulk submit
    /// (server-supplied, true only when Status == Draft)
    /// </summary>
    public bool Selectable { get; set; }
}
