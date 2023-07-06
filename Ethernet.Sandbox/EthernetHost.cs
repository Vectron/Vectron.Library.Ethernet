using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VectronsLibrary.Ethernet.Sandbox;

/// <summary>
/// A <see cref="BackgroundService"/> for opening an ethernet server.
/// </summary>
internal sealed class EthernetHost : BackgroundService
{
    private readonly IEthernetServer ethernetServer;
    private readonly ILogger<EthernetHost> logger;
    private IDisposable? sessionStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetHost"/> class.
    /// </summary>
    /// <param name="logger">A <see cref="ILogger"/>.</param>
    /// <param name="ethernetServer">A <see cref="IEthernetServer"/>.</param>
    public EthernetHost(ILogger<EthernetHost> logger, IEthernetServer ethernetServer)
    {
        this.logger = logger;
        this.ethernetServer = ethernetServer;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        sessionStream?.Dispose();
        base.Dispose();
    }

    /// <inheritdoc/>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ethernetServer.Open();
        sessionStream = ethernetServer.ConnectionStream.Subscribe(async x => await OnClient(x));
        return Task.CompletedTask;
    }

    private async Task OnClient(IConnected<IEthernetConnection> connection)
    {
        await Task.Delay(5000);
        logger.LogInformation("Client state changed {IsConnected}", connection.IsConnected);
    }
}
