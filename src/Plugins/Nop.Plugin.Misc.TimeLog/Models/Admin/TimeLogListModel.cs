using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.TimeLog.Models.Admin;

/// <summary>
/// Represents the time log list model (self-service grid)
/// </summary>
public record TimeLogListModel : BasePagedListModel<TimeLogModel>;
