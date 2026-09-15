namespace Nop.Plugin.Misc.TimeLog.Domain;

/// <summary>
/// Represents the per-record outcome of a bulk submit operation
/// </summary>
public class TimeLogSubmitResult
{
    /// <summary>
    /// Gets or sets the time log identifier this result refers to
    /// </summary>
    public int TimeLogId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the submit succeeded for this record
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the localized error message when <see cref="Success"/> is false
    /// </summary>
    public string ErrorMessage { get; set; }
}
