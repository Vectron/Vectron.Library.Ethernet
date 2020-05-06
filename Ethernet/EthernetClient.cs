using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace VectronsLibrary.Ethernet
{
    public sealed class EthernetClient : Ethernet, IEthernetClient
    {
        private Socket client;
        private bool disposed = false;

        public EthernetClient(ILogger<EthernetClient> logger)
            : base(logger)
        {
        }

        public bool IsConnected => client == null ? false : client.Connected;

        public void ConnectTo(string ip, int port, ProtocolType protocolType)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new ArgumentException("No vallid ip adress specified", nameof(ip));
            }

            if (port <= 0 || port > 65535)
            {
                throw new ArgumentException($"{port} is not a vallid ip4 port number", nameof(port));
            }

            if (client != null)
            {
                logger.LogDebug("Need to close the previous connection first before opening new one");
                Disconnect();
            }

            try
            {
                var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
                logger.LogInformation($"Opening connection to {endpoint}");
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
                client.BeginConnect(endpoint, new AsyncCallback(ConnectCallback), client);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to connect to {client.RemoteEndPoint.ToString()}");
            }
        }

        public void Disconnect()
        {
            if (client == null)
            {
                logger.LogDebug("No connection to close");
                return;
            }

            if (!client.Connected)
            {
                logger.LogDebug("Connection is already closed");
                client = null;
                return;
            }

            logger.LogDebug("Closing connection");
            client.Shutdown(SocketShutdown.Both);
            logger.LogInformation($"{client?.RemoteEndPoint} Connection closed");
            client.Close();
            client = null;
        }

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        public void Send(string message)
        {
            this.Send(client, message);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                logger.LogTrace("Retrieving the socket from the state object");
                var client = (Socket)ar.AsyncState;
                logger.LogTrace("Complete the connection.");
                client.EndConnect(ar);
                logger.LogInformation($"Connected to: {client.RemoteEndPoint}");
                connectionState.OnNext(Connected.Yes(client));
                var state = new StateObject(client);
                logger.LogDebug("Start listening for new messages");
                state.WorkSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }
            catch (SocketException ex)
            {
                logger.LogError(ex, $"Connect failed");
                connectionState.OnNext(Connected.No(client));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Connect failed");
                connectionState.OnNext(Connected.No(client));
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Disconnect();
                    client?.Dispose();
                    client = null;
                }

                disposed = true;
            }
        }
    }
}