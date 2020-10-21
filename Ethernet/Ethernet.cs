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
    public abstract class Ethernet : IEthernet
    {
        protected readonly BehaviorSubject<IConnected<Socket>> connectionState = new BehaviorSubject<IConnected<Socket>>(Connected.No<Socket>(null));
        protected readonly ILogger logger;
        private ISubject<ReceivedData> DataReceived = new Subject<ReceivedData>();

        public Ethernet(ILogger<Ethernet> logger)
        {
            this.logger = logger;
        }

        public IObservable<ReceivedData> ReceivedDataStream => DataReceived.AsObservable();

        public IObservable<IConnected<Socket>> SessionStream => connectionState.AsObservable();

        public abstract void Dispose();

        public virtual void Send(Socket handler, string data)
            => Send(handler, Encoding.ASCII.GetBytes(data));

        public virtual void Send(Socket handler, byte[] data)
        {
            logger.LogDebug("Sending: {0} bytes - To: {1}", data.Length, handler.RemoteEndPoint);
            handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), handler);
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
                    Shutdown(socket);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{0}, Failed receiving data from {1}", ex.Message, remoteEndPoint);
                connectionState.OnNext(Connected.No(socket));
            }
        }

        protected virtual void SendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            var client = (Socket)ar?.AsyncState;
            EndPoint remoteEndPoint = null;
            try
            {
                remoteEndPoint = client.RemoteEndPoint;
                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{0}, Failed sending data to {client.RemoteEndPoint.ToString()}", ex.Message);
                connectionState.OnNext(Connected.No(client));
            }
        }

        protected virtual void Shutdown(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed shutting down socket");
            }

            socket.Close();
            connectionState.OnNext(Connected.No(socket));
        }
    }
}