using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;

namespace VectronsLibrary.Ethernet
{
    public sealed class EthernetServer : Ethernet, IEthernetServer
    {
        private readonly IDisposable clientDisconnectSessionStream;
        private readonly ILogger<EthernetConnection> connectionLogger;
        private readonly List<IEthernetConnection> listClients;

        public EthernetServer(ILogger<EthernetServer> logger, ILogger<EthernetConnection> connectionLogger)
            : base(logger)
        {
            this.connectionLogger = connectionLogger;
            listClients = new List<IEthernetConnection>();
            clientDisconnectSessionStream = SessionStream.Where(x => !x.IsConnected).Subscribe(x => listClients.Remove(x.Value));
        }

        public bool IsOnline => Socket != null && Socket.IsBound;

        public IEnumerable<IEthernetConnection> ListClients => listClients;

        public void Open(string ip, int port, ProtocolType protocolType)
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
                logger.LogDebug("Need to close the previous host first before opening new one");
                Close();
            }

            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
            var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            try
            {
                Socket.Bind(endpoint);
                Socket.Listen(1000);
                logger.LogInformation("Started Listening on: {0}:{1}", ip, port);
                _ = Socket.BeginAccept(AcceptCallback, Socket);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to open server on {0}:{1}", ip, port);
            }
        }

        public void Send(string message)
        {
            foreach (var client in ListClients)
            {
                client.Send(message);
            }
        }

        public void Send(byte[] data)
        {
            foreach (var client in ListClients)
            {
                client.Send(data);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                clientDisconnectSessionStream.Dispose();
                Socket?.Close();
                Socket?.Dispose();
            }
        }

        protected override void Shutdown()
        {
            if (Socket == null)
            {
                logger.LogDebug("No connection to close");
                return;
            }

            if (!Socket.IsBound)
            {
                logger.LogDebug("Connection is already closed");
                Socket = null;
                return;
            }

            var localEndPoint = Socket.LocalEndPoint;
            logger.LogDebug("{0} Closing connection", localEndPoint);
            Socket.Close();
            Socket = null;
            logger.LogInformation("{0} Connection closed", localEndPoint);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                var listener = (Socket)ar.AsyncState;
                var handler = listener.EndAccept(ar);
                _ = listener.BeginAccept(AcceptCallback, listener);
                var ethernetConnection = new EthernetConnection(connectionLogger, handler);
                _ = ethernetConnection.SessionStream.Subscribe(connectionState);
                listClients.Add(ethernetConnection);
                logger.LogInformation("New client connected with adress: {0}", handler.RemoteEndPoint);
            }
            catch (ObjectDisposedException)
            {
                // socket is closed
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to connect");
            }
        }
    }
}