using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.TimeLog.Models.Admin;

/// <summary>
/// Represents a time log entry model for the manager oversight grid (read-only).
/// Same field set as <see cref="TimeLogModel"/> plus the owning customer/staff display,
/// deliberately with no CanEdit/CanDelete/Selectable flags - this grid never renders mutating affordances.
/// </summary>
public record TimeLogAdminModel : BaseNopEntityModel
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; }

    public int ProjectId { get; set; }

    public string ProjectName { get; set; }

    public string Task { get; set; }

    public string Description { get; set; }

    public DateTime Date { get; set; }

    public decimal Time { get; set; }

    public string TimeDisplay { get; set; }

    public int StatusId { get; set; }

    public string StatusName { get; set; }
}
