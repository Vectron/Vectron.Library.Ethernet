using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Vectron.Library.Ethernet;

/// <summary>
/// Implementation of <see cref="IEthernetConnection"/>.
/// </summary>
public sealed partial class EthernetConnection : IEthernetConnection, IDisposable, IAsyncDisposable
{
    private const int BufferSize = 1024;
    private readonly ILogger logger;
    private readonly Socket rawSocket;
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
        rawSocket = socket;
        var publisher = Observable.Create<ReceivedData>(ReceiveDataAsync).Publish();
        ReceivedDataStream = publisher.AsObservable();
        receiveDataConnection = publisher.Connect();
    }

    /// <inheritdoc/>
    public event EventHandler? ConnectionClosed;

    /// <inheritdoc/>
    public bool IsConnected
        => rawSocket != null && rawSocket.Connected;

    /// <inheritdoc/>
    public IObservable<ReceivedData> ReceivedDataStream
    {
        get;
        private set;
    }

    /// <inheritdoc/>
    public Task CloseAsync()
    {
        if (!IsConnected)
        {
            return Task.CompletedTask;
        }

        EndPoint? endPoint = null;
        try
        {
            endPoint = rawSocket.RemoteEndPoint;
        }
        catch (SocketException)
        {
        }

        receiveDataConnection.Dispose();
        rawSocket.Close();
        ConnectionClosed?.Invoke(this, EventArgs.Empty);
        RequestedClose(endPoint);

        return Task.CompletedTask;
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
        await CloseAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SendAsync(ReadOnlyMemory<byte> data)
    {
        ThrowIfDisposed();
        if (!IsConnected)
        {
            return;
        }

        MessageSending(data.Length, rawSocket.RemoteEndPoint);
        try
        {
            _ = await rawSocket.SendAsync(data, SocketFlags.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            FailedToSend(rawSocket.RemoteEndPoint, ex);
            await CloseAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public Task SendAsync(string message)
        => SendAsync(Encoding.ASCII.GetBytes(message));

    private async Task ReceiveDataAsync(IObserver<ReceivedData> observer, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        var receiveBuffer = new byte[BufferSize];
        using var rawBytes = new DisposableArrayPool<byte>();
        var remoteEndPoint = rawSocket.RemoteEndPoint;

        while (!cancellationToken.IsCancellationRequested && rawSocket.Connected)
        {
            var bytesRead = await rawSocket.ReceiveAsync(receiveBuffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
            if (bytesRead <= 0)
            {
                RemoteRequestedClose(remoteEndPoint);
                observer.OnCompleted();
                await CloseAsync().ConfigureAwait(false);
                return;
            }

            rawBytes.Add(receiveBuffer.AsSpan(0, bytesRead));
            if (rawSocket.Available == 0)
            {
                var receivedData = new ReceivedData(rawBytes.Data.ToArray());
                rawBytes.Clear();
                MessageReceived(receivedData, remoteEndPoint);
                observer.OnNext(receivedData);
            }
        }

        observer.OnCompleted();
    }

    /// <summary>
    /// Throw an <see cref="ObjectDisposedException"/> when the object is already disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the object has been disposed.</exception>
    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
