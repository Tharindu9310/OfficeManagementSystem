using Nop.Core;

namespace Nop.Plugin.Misc.TimeLog.Domain;

/// <summary>
/// Represents a mapping between a <see cref="Project"/> and a staff member (<c>Customer</c>)
/// allowed to log time against it
/// </summary>
public class ProjectStaffMapping : BaseEntity
{
    /// <summary>
    /// Gets or sets the project identifier (FK to <see cref="Project"/>)
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the customer (staff member) identifier (FK to <c>Nop.Core.Domain.Customers.Customer</c>)
    /// </summary>
    public int CustomerId { get; set; }
}
