using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.TimeLog.Models.Admin;

/// <summary>
/// Represents a project search model (TT-032/TT-033) - backs the Project List grid
/// </summary>
public record ProjectSearchModel : BaseSearchModel
{
    #region Properties

    [NopResourceDisplayName("Admin.TimeLog.Project.List.Name")]
    public string SearchName { get; set; }

    #endregion
}
