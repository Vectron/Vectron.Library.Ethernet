using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Vectron.Library.Ethernet.Sandbox;

/// <summary>
/// A <see cref="BackgroundService"/> for opening an ethernet server.
/// </summary>
internal sealed partial class EthernetHost : BackgroundService
{
    private readonly IEthernetServer ethernetServer;
    private readonly ILogger<EthernetHost> logger;
    private IDisposable? sessionStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetHost"/> class.
    /// </summary>
    /// <param name="logger">A <see cref="ILogger"/>.</param>
    /// <param name="ethernetServer">A <see cref="IEthernetServer"/>.</param>
    [SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Logging source generator does not support primary constructor")]
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
        sessionStream?.Dispose();
        sessionStream = ethernetServer.ConnectionStream.Subscribe(connection => ClientStateChanged(connection.IsConnected));
        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Client state changed {IsConnected}")]
    private partial void ClientStateChanged(bool isConnected);
}
