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
        private bool disposedValue = false;
        private Socket listener;

        public EthernetServer(ILogger<EthernetServer> logger)
            : base(logger)
        {
            SessionStream.Where(x => !x.IsConnected).Subscribe(x => ListClients.Remove(x.Value));
        }

        public bool IsOnline => listener == null ? false : listener.IsBound;

        public List<Socket> ListClients { get; } = new List<Socket>();

        public void Close()
        {
            if (listener == null)
            {
                logger.LogDebug("No connection to close");
                return;
            }

            if (!listener.IsBound)
            {
                logger.LogDebug("Connection is already closed");
                listener = null;
                return;
            }

            logger.LogDebug($"{listener?.LocalEndPoint} Closing connection");

            foreach (var client in ListClients)
            {
                try
                {
                    client.Shutdown(SocketShutdown.Both);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to disconnect {client?.RemoteEndPoint}");
                }
            }

            logger.LogInformation($"{listener?.LocalEndPoint} Connection closed");
            listener.Close();
            listener = null;
        }

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

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

            if (listener != null)
            {
                logger.LogDebug("Need to close the previous host first before opening new one");
                Close();
            }

            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
            var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            try
            {
                listener.Bind(endpoint);
                listener.Listen(1000);
                logger.LogInformation($"Started Listening on: {ip}:{port}");
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to open server on ip:{ip}:{port}");
            }
        }

        public void Send(string message)
        {
            foreach (var client in ListClients)
            {
                this.Send(client, message);
            }
        }

        protected override void Shutdown(Socket socket)
        {
            base.Shutdown(socket);
            ListClients.Remove(socket);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                var listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                ListClients.Add(handler);
                var state = new StateObject(handler);
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                logger.LogInformation($"New client connected with adress: {handler.RemoteEndPoint}");
                connectionState.OnNext(Connected.Yes(handler));
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to connect");
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                    listener?.Dispose();
                    listener = null;
                }

                disposedValue = true;
            }
        }
    }
}