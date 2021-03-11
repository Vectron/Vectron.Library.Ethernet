using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace VectronsLibrary.Ethernet
{
    public sealed class EthernetClient : EthernetConnection, IEthernetClient
    {
        public EthernetClient(ILogger<EthernetClient> logger)
            : base(logger)
        {
        }

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

            if (Socket != null)
            {
                logger.LogDebug("Need to close the previous connection first before opening new one");
                Close();
            }

            IPEndPoint endpoint = null;
            try
            {
                endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
                logger.LogInformation("Opening connection to {0}", endpoint);
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
                _ = Socket.BeginConnect(endpoint, ConnectCallback, Socket);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to {0}", endpoint);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                logger.LogTrace("Retrieving the socket from the state object");
                var client = (Socket)ar.AsyncState;
                logger.LogTrace("Complete the connection.");
                client.EndConnect(ar);
                logger.LogInformation("Connected to: {0}", client.RemoteEndPoint);
                connectionState.OnNext(Connected.Yes(this));
                var state = new StateObject(client);
                logger.LogDebug("Start listening for new messages");
                _ = client.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
            }
            catch (Exception ex)
            {
                Socket = null;
                logger.LogError(ex, $"Connect failed");
                connectionState.OnNext(Connected.No(this));
            }
        }
    }
}