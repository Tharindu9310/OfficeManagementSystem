using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.TimeLog.Infrastructure;

/// <summary>
/// Registers the Time Log plugin's admin menu items.
/// </summary>
/// <remarks>
/// nopCommerce 5.00 deprecated <c>IAdminMenuPlugin.ManageSiteMapAsync</c> in favor of handling
/// <see cref="AdminMenuCreatedEvent"/> via <c>IConsumer&lt;AdminMenuCreatedEvent&gt;</c> - this is the same
/// mechanism Nop.Plugin.Misc.RFQ.Services.EventConsumer uses in this codebase. Consumers are discovered
/// automatically (no manual DI registration required), matching the RFQ plugin's pattern.
/// </remarks>
public class AdminMenuManager : IConsumer<AdminMenuCreatedEvent>
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IPluginManager<IPlugin> _pluginManager;

    #endregion

    #region Ctor

    public AdminMenuManager(ILocalizationService localizationService,
        IPluginManager<IPlugin> pluginManager)
    {
        _localizationService = localizationService;
        _pluginManager = pluginManager;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handle admin menu created event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var plugin = await _pluginManager.LoadPluginBySystemNameAsync(NopTimeLogDefaults.SystemName);

        //the LoadPluginBySystemNameAsync method returns only plugins that are already fully installed,
        //while the IConsumer<AdminMenuCreatedEvent> event can be called before the installation is complete.
        //unlike IWidgetPlugin/IPaymentMethod etc., a plain IMiscPlugin has no "active system names" settings
        //list, so - as with Nop.Plugin.Misc.RFQ's non-widget menu items - installed is the only check available
        if (plugin == null)
            return;

        var timeLogSection = new AdminMenuItem
        {
            Visible = true,
            SystemName = NopTimeLogDefaults.TimeLogAdminMenuSystemName,
            Title = await _localizationService.GetResourceAsync("Admin.TimeLog.Menu.TimeLog"),
            IconClass = "far fa-clock",
            ChildNodes =
            {
                new AdminMenuItem
                {
                    Visible = true,
                    SystemName = NopTimeLogDefaults.LogTimeAdminMenuSystemName,
                    Title = await _localizationService.GetResourceAsync("Admin.TimeLog.Menu.LogTime"),
                    //controller/routes are built by TT-011; this points at Admin/TimeLog/List and will 404 until then
                    Url = eventMessage.GetMenuItemUrl("TimeLog", "List"),
                    IconClass = "far fa-dot-circle",
                    PermissionNames = new List<string> { PermissionProvider.ManageTimeLog }
                },
                new AdminMenuItem
                {
                    Visible = true,
                    SystemName = NopTimeLogDefaults.TimeLogAllAdminMenuSystemName,
                    Title = await _localizationService.GetResourceAsync("Admin.TimeLog.Menu.TimeLogAll"),
                    //controller/routes are built by TT-012; this points at Admin/TimeLogAdmin/List and will 404 until then
                    Url = eventMessage.GetMenuItemUrl("TimeLogAdmin", "List"),
                    IconClass = "far fa-dot-circle",
                    PermissionNames = new List<string> { PermissionProvider.ManageTimeLogAll }
                },
                new AdminMenuItem
                {
                    Visible = true,
                    SystemName = NopTimeLogDefaults.ProjectAdminMenuSystemName,
                    Title = await _localizationService.GetResourceAsync("Admin.TimeLog.Menu.Project"),
                    //controller/routes are built by TT-033 (ProjectController.List); this points at
                    //Admin/Project/List and will 404 until then - same convention as the two menu
                    //items above did before TT-011/TT-012 landed
                    Url = eventMessage.GetMenuItemUrl("Project", "List"),
                    IconClass = "far fa-folder-open",
                    PermissionNames = new List<string> { PermissionProvider.ManageProjects }
                }
            }
        };

        //insert as a new top-level section, right before the "Third party plugins" root item
        if (!eventMessage.RootMenuItem.InsertBefore("Third party plugins", timeLogSection))
            eventMessage.RootMenuItem.ChildNodes.Add(timeLogSection);
    }

    #endregion
}
