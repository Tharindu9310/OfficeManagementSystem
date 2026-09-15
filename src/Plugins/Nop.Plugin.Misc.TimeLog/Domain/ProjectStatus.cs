namespace Nop.Plugin.Misc.TimeLog.Domain;

/// <summary>
/// Represents a project status
/// </summary>
public enum ProjectStatus
{
    /// <summary>
    /// Not started - project has not begun yet; eligible for new time entries
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// In progress - project is actively underway; eligible for new time entries
    /// </summary>
    Inprogress = 1,

    /// <summary>
    /// On hold - project is temporarily paused; not eligible for new time entries
    /// </summary>
    OnHold = 2,

    /// <summary>
    /// Completed - project work has finished; not eligible for new time entries
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Cancelled - project was cancelled before completion; not eligible for new time entries
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Retired - project was deactivated (manually or automatically, e.g. a blocked delete);
    /// not eligible for new time entries
    /// </summary>
    Retired = 5
}
