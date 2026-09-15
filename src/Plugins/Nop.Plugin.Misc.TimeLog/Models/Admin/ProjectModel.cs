using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.TimeLog.Models.Admin;

/// <summary>
/// Represents a project model (TT-032). Thin view model - never binds directly to
/// <see cref="Domain.Project"/> - backs the Create/Edit standard admin form, which per
/// requirements §6 Decision 1 also doubles as the project "details" view (no separate
/// read-only screen exists).
/// </summary>
public record ProjectModel : BaseNopEntityModel, IStoreMappingSupportedModel
{
    #region Ctor

    public ProjectModel()
    {
        AvailableStatuses = new List<SelectListItem>();
        SelectedCustomerIds = new List<int>();
        AvailableStaff = new List<SelectListItem>();
        AssignedStaff = new List<SelectListItem>();
        SelectedStoreIds = new List<int>();
        AvailableStores = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.TimeLog.Project.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.TimeLog.Project.Fields.StartDate")]
    [UIHint("Date")]
    public DateTime StartDate { get; set; }

    [NopResourceDisplayName("Admin.TimeLog.Project.Fields.EndDate")]
    [UIHint("DateNullable")]
    public DateTime? EndDate { get; set; }

    [NopResourceDisplayName("Admin.TimeLog.Project.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Admin.TimeLog.Project.Fields.Status")]
    public int StatusId { get; set; }

    public List<SelectListItem> AvailableStatuses { get; set; }

    /// <summary>
    /// Gets or sets the localized status display text (List grid display-only, mirrors
    /// TimeLogAdminModel.StatusName's convention - status carries text, not color alone, per WCAG 2.1 AA)
    /// </summary>
    public string StatusName { get; set; }

    /// <summary>
    /// Gets or sets the number of staff members currently assigned to the project (List grid only,
    /// plain numeric count per requirements §7/locked decision - display-only, not posted back)
    /// </summary>
    [NopResourceDisplayName("Admin.TimeLog.Project.List.AssignedStaffCount")]
    public int AssignedStaffCount { get; set; }

    /// <summary>
    /// Gets or sets the posted "assigned" side of the staff dual-listbox (the full desired set of
    /// assigned customer identifiers - see <see cref="Services.IProjectService.SaveStaffAssignmentsAsync"/>)
    /// </summary>
    [NopResourceDisplayName("Admin.TimeLog.Project.Fields.AssignedStaff")]
    public IList<int> SelectedCustomerIds { get; set; }

    /// <summary>
    /// Gets or sets the "available" side of the staff dual-listbox - all Staff-role customers NOT
    /// currently in <see cref="SelectedCustomerIds"/>
    /// </summary>
    public IList<SelectListItem> AvailableStaff { get; set; }

    /// <summary>
    /// Gets or sets the "assigned" side of the staff dual-listbox, rendered from
    /// <see cref="SelectedCustomerIds"/> - kept as a separate list (rather than re-deriving in the view)
    /// so the partial has both lists ready-made
    /// </summary>
    public IList<SelectListItem> AssignedStaff { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the project is limited/restricted to certain stores
    /// (Decision 5 - IStoreMappingSupported)
    /// </summary>
    [NopResourceDisplayName("Admin.TimeLog.Project.Fields.LimitedToStores")]
    public bool LimitedToStores { get; set; }

    public IList<int> SelectedStoreIds { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }

    #endregion
}
