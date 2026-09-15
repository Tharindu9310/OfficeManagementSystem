namespace Nop.Plugin.Misc.TimeLog.Domain;

/// <summary>
/// Represents a time log entry status
/// </summary>
public enum TimeLogStatus
{
    /// <summary>
    /// Draft - entry is editable/deletable by its owner
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Submitted - entry is locked; no un-submit action exists in Phase 1
    /// </summary>
    Submitted = 1
}
