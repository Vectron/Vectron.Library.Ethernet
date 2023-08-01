using System.Collections.Immutable;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vectron.Library.Ethernet;

/// <summary>
/// An Ethernet server implementation.
/// </summary>
public sealed partial class EthernetServer : IEthernetServer, IDisposable, IAsyncDisposable
{
    private readonly List<IEthernetConnection> clients = new();
    private readonly ReaderWriterLockSlim clientsLock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly Subject<IConnected<IEthernetConnection>> connectionStream = new();
    private readonly ILogger<EthernetServer> logger;
    private readonly EthernetServerOptions settings;
    private CancellationTokenSource? cancellationTokenSource;
    private bool disposed;
    private Task? listenTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetServer"/> class.
    /// </summary>
    /// <param name="options">The settings for configuring the <see cref="EthernetServer"/>.</param>
    /// <param name="logger">A <see cref="ILogger"/> instance.</param>
    public EthernetServer(IOptions<EthernetServerOptions> options, ILogger<EthernetServer> logger)
    {
        this.logger = logger;
        settings = options.Value;
    }

    /// <inheritdoc/>
    public IEnumerable<IEthernetConnection> Clients
    {
        get
        {
            clientsLock.EnterReadLock();
            try
            {
                var clone = clients.ToImmutableList();
                return clone;
            }
            finally
            {
                clientsLock.ExitReadLock();
            }
        }
    }

    /// <inheritdoc/>
    public IObservable<IConnected<IEthernetConnection>> ConnectionStream => connectionStream.AsObservable();

    /// <inheritdoc/>
    public bool IsListening
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public Task BroadCastAsync(string message)
        => BroadCastAsync(Encoding.ASCII.GetBytes(message));

    /// <inheritdoc/>
    public Task BroadCastAsync(ReadOnlyMemory<byte> data)
    {
        var sendTasks = clients.Select(x => x.SendAsync(data)).ToArray();
        return Task.WhenAll(sendTasks);
    }

    /// <inheritdoc/>
    public async Task CloseAsync()
    {
        if (cancellationTokenSource == null || listenTask == null)
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            listenTask?.Dispose();
            listenTask = null;
            return;
        }

        cancellationTokenSource.Cancel();
        await listenTask.ConfigureAwait(false);
        listenTask.Dispose();
        listenTask = null;

        cancellationTokenSource.Dispose();
        cancellationTokenSource = null;

        clientsLock.EnterReadLock();
        List<IEthernetConnection>? clone = null;
        try
        {
            clone = new List<IEthernetConnection>(clients);
        }
        finally
        {
            clientsLock.ExitReadLock();
        }

        if (clone != null && clone.Count > 0)
        {
            await Task.WhenAll(clone.Select(x => x.CloseAsync())).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
        => DisposeAsync().AsTask().GetAwaiter().GetResult();

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        await CloseAsync().ConfigureAwait(false);
        connectionStream.Dispose();
        clientsLock.Dispose();
        cancellationTokenSource?.Dispose();
        listenTask?.Dispose();
        disposed = true;
    }

    /// <inheritdoc/>
    public void Open()
    {
        ThrowIfDisposed();
        if (IsListening)
        {
            return;
        }

        cancellationTokenSource?.Dispose();
        cancellationTokenSource = new CancellationTokenSource();
        var endpoint = new IPEndPoint(IPAddress.Parse(settings.IpAddress), settings.Port);
        listenTask = ListenForClient(endpoint, cancellationTokenSource.Token);
    }

    private void EthernetConnection_ConnectionClosed(object? sender, EventArgs e)
    {
        if (sender is EthernetConnection ethernetConnection)
        {
            ethernetConnection.ConnectionClosed -= EthernetConnection_ConnectionClosed;
            clientsLock.EnterWriteLock();
            try
            {
                _ = clients.Remove(ethernetConnection);
            }
            finally
            {
                clientsLock.ExitWriteLock();
            }

            connectionStream.OnNext(Connected.No(ethernetConnection));
        }
    }

    private async Task ListenForClient(IPEndPoint endPoint, CancellationToken cancellationToken)
    {
        try
        {
            using var rawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, settings.ProtocolType);
            rawSocket.Bind(endPoint);
            rawSocket.Listen(1000);
            IsListening = true;
            StartListening(endPoint);

            while (!cancellationToken.IsCancellationRequested)
            {
                var clientSocket = await rawSocket.AcceptAsync(cancellationToken).ConfigureAwait(false);
                var ethernetConnection = new EthernetConnection(logger, clientSocket);
                ethernetConnection.ConnectionClosed += EthernetConnection_ConnectionClosed;

                clientsLock.EnterWriteLock();
                try
                {
                    clients.Add(ethernetConnection);
                }
                finally
                {
                    clientsLock.ExitWriteLock();
                }

                connectionStream.OnNext(Connected.Yes(ethernetConnection));
                ClientConnected(clientSocket.RemoteEndPoint);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (SocketException ex)
        {
            FailedToListen(endPoint, ex.Message);
        }
        finally
        {
            StoppedListening(endPoint);
            IsListening = false;
        }
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
