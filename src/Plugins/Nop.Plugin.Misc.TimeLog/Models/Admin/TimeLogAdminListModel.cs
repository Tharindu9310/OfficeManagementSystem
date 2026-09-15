using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.TimeLog.Models.Admin;

/// <summary>
/// Represents the time log list model for the manager oversight grid
/// </summary>
public record TimeLogAdminListModel : BasePagedListModel<TimeLogAdminModel>;
