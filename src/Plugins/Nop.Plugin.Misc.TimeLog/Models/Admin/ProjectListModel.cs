using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.TimeLog.Models.Admin;

/// <summary>
/// Represents the project list model for the Project List grid
/// </summary>
public record ProjectListModel : BasePagedListModel<ProjectModel>;
