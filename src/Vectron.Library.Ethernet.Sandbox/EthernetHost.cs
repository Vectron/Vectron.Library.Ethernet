using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Vectron.Library.Ethernet.Sandbox;

/// <summary>
/// A <see cref="BackgroundService"/> for opening an ethernet server.
/// </summary>
/// <param name="logger">A <see cref="ILogger"/>.</param>
/// <param name="ethernetServer">A <see cref="IEthernetServer"/>.</param>
internal sealed partial class EthernetHost(ILogger<EthernetHost> logger, IEthernetServer ethernetServer) : BackgroundService
{
    private readonly ILogger<EthernetHost> logger = logger;
    private IDisposable? sessionStream;

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
        sessionStream?.Dispose();
        sessionStream = ethernetServer.ConnectionStream.Subscribe(connection => ClientStateChanged(connection.IsConnected));
        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Client state changed {IsConnected}")]
    private partial void ClientStateChanged(bool isConnected);
}
