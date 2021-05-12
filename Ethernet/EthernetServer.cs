using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;

namespace VectronsLibrary.Ethernet
{
    /// <summary>
    /// An Ethernet server implementation.
    /// </summary>
    public sealed class EthernetServer : Ethernet, IEthernetServer
    {
        private readonly IDisposable clientDisconnectSessionStream;
        private readonly ILogger<EthernetConnection> connectionLogger;
        private readonly List<IEthernetConnection> listClients;

        /// <summary>
        /// Initializes a new instance of the <see cref="EthernetServer"/> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> instance used for logging.</param>
        /// <param name="connectionLogger">An <see cref="ILogger"/> instance used for logging in connections.</param>
        public EthernetServer(ILogger<EthernetServer> logger, ILogger<EthernetConnection> connectionLogger)
            : base(logger)
        {
            this.connectionLogger = connectionLogger;
            listClients = new List<IEthernetConnection>();
            clientDisconnectSessionStream = SessionStream.Where(x => !x.IsConnected && x.Value != null).Subscribe(x => listClients.Remove(x.Value!));
        }

        /// <inheritdoc/>
        public bool IsOnline => Socket != null && Socket.IsBound;

        /// <inheritdoc/>
        public IEnumerable<IEthernetConnection> ListClients => listClients;

        /// <inheritdoc/>
        public void Open(string ip, int port, ProtocolType protocolType)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new ArgumentException("No vallid ip adress specified", nameof(ip));
            }

            if (port is <= 0 or > 65535)
            {
                throw new ArgumentException($"{port} is not a vallid ip4 port number", nameof(port));
            }

            if (Socket != null)
            {
                Logger.LogDebug("Need to close the previous host first before opening new one");
                Close();
            }

            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
            var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            try
            {
                Socket.Bind(endpoint);
                Socket.Listen(1000);
                Logger.LogInformation("Started Listening on: {0}:{1}", ip, port);
                _ = Socket.BeginAccept(AcceptCallback, Socket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to open server on {0}:{1}", ip, port);
            }
        }

        /// <inheritdoc/>
        public void Send(string message)
        {
            foreach (var client in ListClients)
            {
                client.Send(message);
            }
        }

        /// <inheritdoc/>
        public void Send(byte[] data)
        {
            foreach (var client in ListClients)
            {
                client.Send(data);
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void Shutdown()
        {
            if (Socket == null)
            {
                Logger.LogDebug("No connection to close");
                return;
            }

            if (!Socket.IsBound)
            {
                Logger.LogDebug("Connection is already closed");
                Socket = null;
                return;
            }

            var localEndPoint = Socket.LocalEndPoint;
            Logger.LogDebug("{0} Closing connection", localEndPoint);
            Socket.Close();
            Socket = null;
            Logger.LogInformation("{0} Connection closed", localEndPoint);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                if (ar.AsyncState is not Socket listener)
                {
                    Logger.LogCritical("No Socket was passed to the state object");
                    return;
                }

                var handler = listener.EndAccept(ar);
                _ = listener.BeginAccept(AcceptCallback, listener);
                var ethernetConnection = new EthernetConnection(connectionLogger, handler);
                _ = ethernetConnection.SessionStream.Subscribe(ConnectionState);
                listClients.Add(ethernetConnection);
                Logger.LogInformation("New client connected with adress: {0}", handler.RemoteEndPoint);
            }
            catch (ObjectDisposedException)
            {
                // socket is closed
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to connect");
            }
        }
    }
}