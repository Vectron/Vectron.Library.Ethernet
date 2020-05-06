using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
            logger.LogDebug($"Sending: {data.Length} bytes - To: {handler.RemoteEndPoint.ToString()}");
            handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        protected virtual void ReceiveCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the client socket
            // from the asynchronous state object.
            var state = (StateObject)ar.AsyncState;
            Socket socket = state.WorkSocket;
            try
            {
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
                        logger.LogDebug($"Received: {receivedData.Message} - From: {socket.RemoteEndPoint.ToString()}");
                        state.RawBytes.Clear();
                    }

                    // Get the rest of the data.
                    _ = socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    logger.LogInformation($"{socket.RemoteEndPoint} requested a shutdown");
                    Shutdown(socket);
                }
            }
            catch (ObjectDisposedException ex)
            {
                if (socket.Connected)
                {
                    logger.LogError($"{ex.Message}, Failed receiving data from {socket.RemoteEndPoint}");
                    connectionState.OnNext(Connected.No(socket));
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{ex.Message}, Failed receiving data from {socket.RemoteEndPoint}");
                connectionState.OnNext(Connected.No(socket));
            }
        }

        protected virtual void SendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            var client = (Socket)ar.AsyncState;
            try
            {
                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception ex)
            {
                logger.LogError($"{ex.Message}, Failed sending data to {client.RemoteEndPoint.ToString()}");
                connectionState.OnNext(Connected.No(client));
            }
        }

        protected virtual void Shutdown(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
            }

            socket.Close();
            connectionState.OnNext(Connected.No(socket));
        }
    }
}