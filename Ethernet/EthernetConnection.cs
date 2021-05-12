using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Microsoft.Extensions.Logging;

namespace VectronsLibrary.Ethernet
{
    /// <summary>
    /// Implementation of <see cref="IEthernetConnection"/>.
    /// </summary>
    public class EthernetConnection : Ethernet, IEthernetConnection
    {
        private readonly ISubject<ReceivedData> dataReceived = new Subject<ReceivedData>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EthernetConnection"/> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> instance used for logging.</param>
        public EthernetConnection(ILogger<EthernetConnection> logger)
            : base(logger)
        {
            Socket = null;
            ConnectionState.OnNext(Connected.No(this));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EthernetConnection"/> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> instance used for logging.</param>
        /// <param name="socket">The raw socket used to communicate.</param>
        public EthernetConnection(ILogger<EthernetConnection> logger, Socket socket)
            : base(logger)
        {
            Socket = socket;
            ConnectionState.OnNext(Connected.Yes(this));
            if (socket != null)
            {
                var state = new StateObject(socket);
                _ = socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
            }
        }

        /// <inheritdoc/>
        public bool IsConnected => Socket != null && Socket.Connected;

        /// <inheritdoc/>
        public IObservable<ReceivedData> ReceivedDataStream => dataReceived.AsObservable();

        /// <inheritdoc/>
        public virtual void Send(string data)
        {
            Send(Encoding.ASCII.GetBytes(data));
        }

        /// <inheritdoc/>
        public virtual void Send(byte[] data)
        {
            if (IsConnected)
            {
                Logger.LogDebug("Sending: {0} bytes - To: {1}", data.Length, Socket?.RemoteEndPoint);
                _ = Socket?.BeginSend(data, 0, data.Length, 0, SendCallback, Socket);
            }
        }

        /// <summary>
        /// A callback function when data is received from the socket.
        /// </summary>
        /// <param name="ar"><see cref="IAsyncResult"/>.</param>
        protected virtual void ReceiveCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the client socket
            // from the asynchronous state object.
            if (ar.AsyncState is not StateObject state)
            {
                Logger.LogCritical("No StateObject was passed to the state object");
                return;
            }

            var socket = state.WorkSocket;
            EndPoint? remoteEndPoint = null;

            try
            {
                remoteEndPoint = socket.RemoteEndPoint;

                // Read data from the remote device.
                var bytesRead = socket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    for (var i = 0; i < bytesRead; i++)
                    {
                        state.RawBytes.Add(state.Buffer[i]);
                    }

                    if (socket.Available == 0)
                    {
                        var receivedData = new ReceivedData(state.RawBytes.ToArray(), socket);
                        dataReceived.OnNext(receivedData);
                        Logger.LogDebug("Received: {0} - From: {1}", receivedData.Message, remoteEndPoint);
                        state.RawBytes.Clear();
                    }

                    // Get the rest of the data.
                    _ = socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    Logger.LogInformation("{0} requested a shutdown", remoteEndPoint);
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{0}, Failed receiving data from {1}", ex.Message, remoteEndPoint);
                ConnectionState.OnNext(Connected.No(this));
            }
        }

        /// <summary>
        /// A callback function when data is send to the socket.
        /// </summary>
        /// <param name="ar"><see cref="IAsyncResult"/>.</param>
        protected virtual void SendCallback(IAsyncResult ar)
        {
            if (ar is null)
            {
                throw new ArgumentNullException(nameof(ar));
            }

            // Retrieve the socket from the state object.
            if (ar.AsyncState is not Socket client)
            {
                Logger.LogCritical("No Socket was passed to the send function");
                return;
            }

            try
            {
                // Complete sending the data to the remote device.
                var bytesSent = client.EndSend(ar);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{0}, Failed sending data to {client.RemoteEndPoint.ToString()}", ex.Message);
                ConnectionState.OnNext(Connected.No(this));
            }
        }

        /// <inheritdoc/>
        protected override void Shutdown()
        {
            try
            {
                if (Socket == null)
                {
                    Logger.LogDebug("No connection to close");
                    return;
                }

                if (!Socket.Connected)
                {
                    Logger.LogDebug("Connection is already closed");
                    Socket = null;
                    return;
                }

                Logger.LogDebug("Closing connection");
                Socket.Disconnect(false);
                ConnectionState.OnNext(Connected.No(this));
                Logger.LogInformation("{0} Connection closed", Socket.RemoteEndPoint);
                Socket.Close();
                Socket = null;
            }
            catch (ObjectDisposedException ex)
            {
                Logger.LogDebug(ex, "Connection is already closed");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed shutting down socket");
            }
        }
    }
}