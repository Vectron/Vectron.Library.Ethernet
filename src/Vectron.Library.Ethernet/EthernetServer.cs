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
public sealed partial class EthernetServer : IEthernetServer, IDisposable
{
    private readonly List<IEthernetConnection> clients = new();
    private readonly ReaderWriterLockSlim clientsLock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly Subject<IConnected<IEthernetConnection>> connectionStream = new();
    private readonly ILogger<EthernetServer> logger;
    private readonly Socket rawSocket;
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
        rawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, settings.ProtocolType);
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
    public bool IsListening => rawSocket != null && rawSocket.IsBound;

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
        if (!IsListening)
        {
            return;
        }

        var localEndPoint = rawSocket.LocalEndPoint;
        cancellationTokenSource?.Cancel();
        if (listenTask != null)
        {
            await listenTask.ConfigureAwait(false);
            listenTask?.Dispose();
            listenTask = null;
        }

        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
        clientsLock.EnterWriteLock();
        try
        {
            clients.Clear();
        }
        finally
        {
            clientsLock.ExitWriteLock();
        }

        rawSocket.Close();
        StoppedListening(localEndPoint);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        CloseAsync().GetAwaiter().GetResult();

        connectionStream.Dispose();
        clientsLock.Dispose();
        rawSocket.Dispose();
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

        try
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(settings.IpAddress), settings.Port);
            rawSocket.Bind(endpoint);
            rawSocket.Listen(1000);

            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            listenTask = ListenForClient(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            FailedToListen(settings.IpAddress, settings.Port, ex);
        }
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

    private async Task ListenForClient(CancellationToken cancellationToken)
    {
        try
        {
            var localEndPoint = rawSocket.LocalEndPoint;
            StartListening(localEndPoint);
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
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
