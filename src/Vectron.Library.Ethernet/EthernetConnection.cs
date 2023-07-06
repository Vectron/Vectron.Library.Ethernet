using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Vectron.Library.Ethernet;

/// <summary>
/// Implementation of <see cref="IEthernetConnection"/>.
/// </summary>
public partial class EthernetConnection : IEthernetConnection, IDisposable
{
    private const int BufferSize = 1024;
    private readonly ILogger logger;
    private readonly IDisposable receiveDataConnection;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetConnection"/> class.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> instance used for logging.</param>
    /// <param name="socket">The raw socket used to communicate.</param>
    public EthernetConnection(ILogger logger, Socket socket)
    {
        this.logger = logger;
        RawSocket = socket;
        var publisher = Observable.Create<ReceivedData>(ReceiveDataAsync).Publish();
        ReceivedDataStream = publisher.AsObservable();
        receiveDataConnection = publisher.Connect();
    }

    /// <inheritdoc/>
    public event EventHandler? ConnectionClosed;

    /// <inheritdoc/>
    public bool IsConnected
        => RawSocket != null && RawSocket.Connected;

    /// <inheritdoc/>
    public IObservable<ReceivedData> ReceivedDataStream
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the underlying <see cref="Socket"/>.
    /// </summary>
    protected Socket RawSocket
    {
        get;
        init;
    }

    /// <inheritdoc/>
    public Task CloseAsync()
    {
        if (!IsConnected)
        {
            return Task.CompletedTask;
        }

        var endPoint = RawSocket.RemoteEndPoint;
        receiveDataConnection.Dispose();
        RawSocket.Close();
        ConnectionClosed?.Invoke(this, EventArgs.Empty);
        RequestedClose(endPoint);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        if (disposed)
        {
            return;
        }

        CloseAsync().GetAwaiter().GetResult();

        disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async Task SendAsync(ReadOnlyMemory<byte> data)
    {
        ThrowIfDisposed();
        if (!IsConnected)
        {
            return;
        }

        MessageSending(data.Length, RawSocket.RemoteEndPoint);
        try
        {
            _ = await RawSocket.SendAsync(data, SocketFlags.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            FailedToSend(RawSocket.RemoteEndPoint, ex);
            await CloseAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public Task SendAsync(string message)
        => SendAsync(Encoding.ASCII.GetBytes(message));

    /// <summary>
    /// Throw an <see cref="ObjectDisposedException"/> when the object is already disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the object has been disposed.</exception>
    protected virtual void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }

    private async Task ReceiveDataAsync(IObserver<ReceivedData> observer, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        while (!RawSocket.Connected
            && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
        }

        var receiveBuffer = new byte[BufferSize];
        using var rawBytes = new DisposableArrayPool<byte>();
        var remoteEndPoint = RawSocket.RemoteEndPoint;

        while (!cancellationToken.IsCancellationRequested)
        {
            var bytesRead = await RawSocket.ReceiveAsync(receiveBuffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
            if (bytesRead < 0)
            {
                RemoteRequestedClose(remoteEndPoint);
                observer.OnCompleted();
                await CloseAsync().ConfigureAwait(false);
                return;
            }

            rawBytes.Add(receiveBuffer.AsSpan(0, bytesRead));
            if (RawSocket.Available == 0)
            {
                var receivedData = new ReceivedData(rawBytes.Data.ToArray());
                rawBytes.Clear();
                MessageReceived(receivedData, remoteEndPoint);
                observer.OnNext(receivedData);
            }
        }

        observer.OnCompleted();
    }
}
