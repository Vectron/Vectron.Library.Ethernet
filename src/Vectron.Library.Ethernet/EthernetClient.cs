using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vectron.Library.Ethernet;

/// <summary>
/// Default implementation of <see cref="IEthernetClient"/>.
/// </summary>
public sealed partial class EthernetClient : IEthernetConnection, IEthernetClient, IDisposable, IAsyncDisposable
{
    private readonly ILogger logger;
    private readonly EthernetClientOptions settings;
    private bool disposed;
    private EthernetConnection? ethernetConnection;

    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetClient"/> class.
    /// </summary>
    /// <param name="options">The settings for configuring the <see cref="EthernetClient"/>.</param>
    /// <param name="logger">A <see cref="ILogger"/> instance.</param>
    public EthernetClient(IOptions<EthernetClientOptions> options, ILogger<EthernetClient> logger)
    {
        this.logger = logger;
        settings = options.Value;
    }

    /// <inheritdoc/>
    public event EventHandler? ConnectionClosed;

    /// <inheritdoc/>
    public bool IsConnected => ethernetConnection != null && ethernetConnection.IsConnected;

    /// <inheritdoc/>
    public IObservable<ReceivedData> ReceivedDataStream => ethernetConnection == null ? Observable.Empty<ReceivedData>() : ethernetConnection.ReceivedDataStream;

    /// <inheritdoc/>
    public Task CloseAsync()
    {
        if (ethernetConnection == null)
        {
            return Task.CompletedTask;
        }

        return ethernetConnection.CloseAsync();
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
            if (ethernetConnection != null)
            {
                await ethernetConnection.DisposeAsync().ConfigureAwait(false);
                ethernetConnection = null;
            }

            var endpoint = new IPEndPoint(IPAddress.Parse(settings.IpAddress), settings.Port);
            StartingToConnect(endpoint);
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, settings.ProtocolType);
            ethernetConnection = new EthernetConnection(logger, clientSocket);
            ethernetConnection.ConnectionClosed += EthernetConnection_ConnectionClosed;

            await clientSocket.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
            ConnectedTo(endpoint);
        }
        catch (SocketException ex)
        {
            FailedToConnect(settings.IpAddress, settings.Port, ex.Message);
        }

        return IsConnected;
    }

    /// <inheritdoc/>
    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        if (ethernetConnection != null)
        {
            await ethernetConnection.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public Task SendAsync(ReadOnlyMemory<byte> data)
    {
        if (ethernetConnection == null)
        {
            return Task.CompletedTask;
        }

        return ethernetConnection.SendAsync(data);
    }

    /// <inheritdoc/>
    public Task SendAsync(string message)
    {
        if (ethernetConnection == null)
        {
            return Task.CompletedTask;
        }

        return ethernetConnection.SendAsync(message);
    }

    private void EthernetConnection_ConnectionClosed(object? sender, EventArgs e)
                        => ConnectionClosed?.Invoke(this, e);

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
