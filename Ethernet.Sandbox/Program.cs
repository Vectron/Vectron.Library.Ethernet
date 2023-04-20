using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VectronsLibrary.Ethernet.Sandbox;

/// <summary>
/// Main entry point.
/// </summary>
internal sealed class Program
{
    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        => _ = builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "hh:mm:ss:fff ";
        });

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        _ = services.AddScoped<IEthernetServer, EthernetServer>();
        _ = services.AddHostedService<EthernetHost>();
    }

    private static Task Main(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureLogging(ConfigureLogging)
            .ConfigureServices(ConfigureServices)
            .RunConsoleAsync(CancellationToken.None);
}