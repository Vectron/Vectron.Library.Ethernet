using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vectron.Library.Ethernet;

/// <summary>
/// Default implementation of <see cref="IEthernetClient"/>.
/// </summary>
public sealed partial class EthernetClient : EthernetConnection, IEthernetClient
{
    private readonly ILogger logger;
    private readonly EthernetClientOptions settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetClient"/> class.
    /// </summary>
    /// <param name="options">The settings for configuring the <see cref="EthernetClient"/>.</param>
    /// <param name="logger">A <see cref="ILogger"/> instance.</param>
    public EthernetClient(IOptions<EthernetClientOptions> options, ILogger<EthernetClient> logger)
        : base(logger, new Socket(AddressFamily.InterNetwork, SocketType.Stream, options.Value.ProtocolType))
    {
        this.logger = logger;
        settings = options.Value;
    }

    /// <inheritdoc/>
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            return true;
        }

        try
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(settings.IpAddress), settings.Port);
            StartingToConnect(endpoint);
            await RawSocket.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
            ConnectedTo(endpoint);
        }
        catch (Exception ex)
        {
            FailedToConnect(settings.IpAddress, settings.Port, ex);
            throw;
        }

        return IsConnected;
    }
}
