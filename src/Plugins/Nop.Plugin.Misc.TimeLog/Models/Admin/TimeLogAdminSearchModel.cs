using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.TimeLog.Models.Admin;

/// <summary>
/// Represents a time log search model for the manager oversight grid (read-only, cross-staff).
/// Adds a Customer/staff filter on top of the self-service filter set.
/// </summary>
public record TimeLogAdminSearchModel : BaseSearchModel
{
    #region Ctor

    public TimeLogAdminSearchModel()
    {
        AvailableStatuses = new List<SelectListItem>();
        AvailableProjects = new List<SelectListItem>();
        AvailableCustomers = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    //ENH-011: same fix/pattern as ENH-001's TimeLogSearchModel.DateFrom/DateTo - [UIHint("DateNullable")]
    //selects EditorTemplates/DateNullable.cshtml (a plain type="date" input) instead of the default
    //DateTimeNullable.cshtml template (type="datetime-local", which adds an unwanted time-of-day part).
    //Confirmed the oversight model's Date filter properties are named/typed identically (nullable DateTime
    //DateFrom/DateTo) before mirroring the exact attribute placement.
    [NopResourceDisplayName("Admin.TimeLog.Filter.DateFrom")]
    [UIHint("DateNullable")]
    public DateTime? DateFrom { get; set; }

    [NopResourceDisplayName("Admin.TimeLog.Filter.DateTo")]
    [UIHint("DateNullable")]
    public DateTime? DateTo { get; set; }

    [NopResourceDisplayName("Admin.TimeLog.Filter.Status")]
    public int StatusId { get; set; }

    public List<SelectListItem> AvailableStatuses { get; set; }

    [NopResourceDisplayName("Admin.TimeLog.Filter.Project")]
    public int ProjectId { get; set; }

    public List<SelectListItem> AvailableProjects { get; set; }

    /// <summary>
    /// Gets or sets the staff/customer filter (0 = all customers)
    /// </summary>
    [NopResourceDisplayName("Admin.TimeLog.Filter.Customer")]
    public int CustomerId { get; set; }

    public List<SelectListItem> AvailableCustomers { get; set; }

    #endregion
}
