using System.Diagnostics.CodeAnalysis;
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
    private readonly IOptionsSnapshot<EthernetClientOptions> options;
    private bool disposed;
    private EthernetConnection? ethernetConnection;

    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetClient"/> class.
    /// </summary>
    /// <param name="options">The settings for configuring the <see cref="EthernetClient"/>.</param>
    /// <param name="logger">A <see cref="ILogger"/> instance.</param>
    [SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Logging source generator does not support primary constructor")]
    public EthernetClient(IOptionsSnapshot<EthernetClientOptions> options, ILogger<EthernetClient> logger)
    {
        this.options = options;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public event EventHandler? ConnectionClosed;

    /// <inheritdoc/>
    public bool IsConnected => ethernetConnection != null && ethernetConnection.IsConnected;

    /// <inheritdoc/>
    public EndPoint? LocalEndPoint => ethernetConnection?.LocalEndPoint;

    /// <inheritdoc/>
    public IObservable<ReceivedData> ReceivedDataStream => ethernetConnection == null ? Observable.Empty<ReceivedData>() : ethernetConnection.ReceivedDataStream;

    /// <inheritdoc/>
    public EndPoint? RemoteEndPoint => ethernetConnection?.RemoteEndPoint;

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
    public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Get(name: null);
        var endpoint = new IPEndPoint(IPAddress.Parse(settings.IpAddress), settings.Port);
        return ConnectAsync(endpoint, settings.ProtocolType, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ConnectAsync(IPEndPoint endpoint, ProtocolType protocolType, CancellationToken cancellationToken = default)
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

            StartingToConnect(endpoint);
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
            await clientSocket.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
            ethernetConnection = new EthernetConnection(logger, clientSocket);
            ethernetConnection.ConnectionClosed += EthernetConnection_ConnectionClosed;

            ConnectedTo(endpoint);
        }
        catch (SocketException ex)
        {
            FailedToConnect(endpoint.Address.ToString(), endpoint.Port, ex.Message);
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
#if NET7_0_OR_GREATER
        => ObjectDisposedException.ThrowIf(disposed, this);
#else
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }

#endif
}
