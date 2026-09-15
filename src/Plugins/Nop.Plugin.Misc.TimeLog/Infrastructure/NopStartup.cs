using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.TimeLog.Services;

namespace Nop.Plugin.Misc.TimeLog.Infrastructure;

/// <summary>
/// Represents the plugin's DI registration, following the same <see cref="INopStartup"/>
/// pattern used by Nop.Plugin.Misc.RFQ (PluginNopStartup) in this codebase.
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITimeLogService, TimeLogService>();

        // TimeLogValidator (an AbstractValidator<T>) is auto-registered by
        // Nop.Web.Framework's AddNopMvc() -> AddValidatorsFromAssemblies() scan of all
        // Nop.* assemblies. Registering it again here would duplicate the service
        // registration and trigger the "PluginsOverrideSameService" admin warning.
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 3000;
}
