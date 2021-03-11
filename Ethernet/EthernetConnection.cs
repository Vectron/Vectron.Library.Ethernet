using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace VectronsLibrary.Ethernet
{
    public class EthernetConnection : Ethernet, IEthernetConnection
    {
        private readonly ISubject<ReceivedData> DataReceived = new Subject<ReceivedData>();

        public EthernetConnection(ILogger<EthernetConnection> logger)
            : base(logger)
        {
            Socket = null;
            connectionState.OnNext(Connected.No(this));
        }

        public EthernetConnection(ILogger<EthernetConnection> logger, Socket socket)
            : base(logger)
        {
            Socket = socket;
            connectionState.OnNext(Connected.Yes(this));
            if (socket != null)
            {
                var state = new StateObject(socket);
                _ = socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
            }
        }

        public bool IsConnected => Socket != null && Socket.Connected;

        public IObservable<ReceivedData> ReceivedDataStream => DataReceived.AsObservable();

        public virtual void Send(string data)
            => Send(Encoding.ASCII.GetBytes(data));

        public virtual void Send(byte[] data)
        {
            if (IsConnected)
            {
                logger.LogDebug("Sending: {0} bytes - To: {1}", data.Length, Socket?.RemoteEndPoint);
                _ = Socket?.BeginSend(data, 0, data.Length, 0, SendCallback, Socket);
            }
        }

        protected virtual void ReceiveCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the client socket
            // from the asynchronous state object.
            var state = (StateObject)ar.AsyncState;
            Socket socket = state.WorkSocket;
            EndPoint remoteEndPoint = null;

            try
            {
                remoteEndPoint = socket.RemoteEndPoint;
                // Read data from the remote device.
                int bytesRead = socket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    for (int i = 0; i < bytesRead; i++)
                    {
                        state.RawBytes.Add(state.Buffer[i]);
                    }

                    if (socket.Available == 0)
                    {
                        var receivedData = new ReceivedData(state.RawBytes.ToArray(), socket);
                        DataReceived.OnNext(receivedData);
                        logger.LogDebug("Received: {0} - From: {1}", receivedData.Message, remoteEndPoint);
                        state.RawBytes.Clear();
                    }

                    // Get the rest of the data.
                    _ = socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    logger.LogInformation("{0} requested a shutdown", remoteEndPoint);
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{0}, Failed receiving data from {1}", ex.Message, remoteEndPoint);
                connectionState.OnNext(Connected.No(this));
            }
        }

        protected virtual void SendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            var client = (Socket)ar?.AsyncState;
            try
            {
                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{0}, Failed sending data to {client.RemoteEndPoint.ToString()}", ex.Message);
                connectionState.OnNext(Connected.No(this));
            }
        }

        protected override void Shutdown()
        {
            try
            {
                if (Socket == null)
                {
                    logger.LogDebug("No connection to close");
                    return;
                }

                if (!Socket.Connected)
                {
                    logger.LogDebug("Connection is already closed");
                    Socket = null;
                    return;
                }

                logger.LogDebug("Closing connection");
                Socket.Disconnect(false);
                connectionState.OnNext(Connected.No(this));
                logger.LogInformation("{0} Connection closed", Socket.RemoteEndPoint);
                Socket.Close();
                Socket = null;
            }
            catch (ObjectDisposedException ex)
            {
                logger.LogDebug(ex, "Connection is already closed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed shutting down socket");
            }
        }
    }
}