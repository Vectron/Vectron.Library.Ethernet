using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VectronsLibrary.Ethernet;

/// <summary>
/// Extension methods for adding ethernet services to the DI container.
/// </summary>
public static class EthernetServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services for an ethernet client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEthernetClient(this IServiceCollection services)
    {
        services.TryAddScoped<IEthernetClient, EthernetClient>();
        _ = services.ConfigureOptions<EthernetClientOptionsDefaults>();
        return services;
    }

    /// <summary>
    /// Adds the services for an ethernet client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configure">
    /// A delegate to configure the <see cref="EthernetClientOptions"/> options for the client.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEthernetClient(this IServiceCollection services, Action<EthernetClientOptions> configure)
    {
        services.TryAddScoped<IEthernetClient, EthernetClient>();
        _ = services.ConfigureOptions<EthernetClientOptionsDefaults>();
        _ = services.Configure(configure);
        return services;
    }

    /// <summary>
    /// Adds the services for an ethernet server.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEthernetServer(this IServiceCollection services)
    {
        services.TryAddScoped<IEthernetServer, EthernetServer>();
        _ = services.ConfigureOptions<EthernetServerOptionsDefaults>();
        return services;
    }

    /// <summary>
    /// Adds the services for an ethernet server.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configure">
    /// A delegate to configure the <see cref="EthernetClientOptions"/> options for the client.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEthernetServer(this IServiceCollection services, Action<EthernetServerOptions> configure)
    {
        services.TryAddScoped<IEthernetServer, EthernetServer>();
        _ = services.ConfigureOptions<EthernetServerOptionsDefaults>();
        _ = services.Configure(configure);
        return services;
    }
}
