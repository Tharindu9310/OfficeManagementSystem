using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.TimeLog.Models.Admin;

/// <summary>
/// Represents a time log search model (self-service grid - always scoped to the current customer server-side)
/// </summary>
public record TimeLogSearchModel : BaseSearchModel
{
    #region Ctor

    public TimeLogSearchModel()
    {
        AvailableStatuses = new List<SelectListItem>();
        AvailableProjects = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    //ENH-001: [UIHint("DateNullable")] is the real date-only search-filter convention used throughout this
    //codebase's admin area (confirmed against Admin.Areas.Orders.OrderSearchModel.StartDate/EndDate) - it
    //selects EditorTemplates/DateNullable.cshtml (a plain type="date" input), instead of falling through to
    //the default DateTimeNullable.cshtml template (type="datetime-local", which adds an unwanted time part).
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

    #endregion
}
