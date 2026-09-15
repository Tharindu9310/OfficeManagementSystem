using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core;

namespace Nop.Plugin.Misc.TimeLog.Domain;

/// <summary>
/// Represents a time log entry
/// </summary>
public class TimeLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the customer (owner) identifier.
    /// Always set server-side from the current customer - never trusted from client input.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the project identifier (FK to <see cref="Domain.Project"/>)
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the task name/description of work performed
    /// </summary>
    public string Task { get; set; }

    /// <summary>
    /// Gets or sets the free-text description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the date the time was logged for
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the number of hours logged, as a decimal (e.g. 6.5 = 6 hours 30 minutes).
    /// Column type is <c>decimal(9,6)</c> (resolved for TT-003 - supersedes an earlier decimal(5,4)
    /// draft in the design doc). No rounding is applied when converting to/from HH:mm display;
    /// the value is persisted and validated exactly as entered (round-tripped via
    /// minutes = round(hours * 60), hours = minutes / 60) so values such as 20 minutes
    /// (a non-terminating decimal fraction of an hour) reconstruct exactly. Do not
    /// reintroduce 2-decimal-place rounding anywhere in the read/write path.
    /// </summary>
    public decimal Time { get; set; }

    /// <summary>
    /// Gets or sets the time log status identifier (backing field for <see cref="Status"/>)
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the time log status
    /// </summary>
    [NotMapped]
    public TimeLogStatus Status
    {
        get => (TimeLogStatus)StatusId;
        set => StatusId = (int)value;
    }

    /// <summary>
    /// Gets or sets the date and time (UTC) the entry was created
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time (UTC) the entry was last updated
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }
}
