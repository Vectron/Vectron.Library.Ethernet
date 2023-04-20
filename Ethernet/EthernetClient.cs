using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace VectronsLibrary.Ethernet;

/// <summary>
/// Default implementation of <see cref="IEthernetClient"/>.
/// </summary>
public sealed class EthernetClient : Ethernet, IEthernetClient
{
    private readonly ILoggerFactory loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetClient"/> class.
    /// </summary>
    /// <param name="loggerFactory">An <see cref="ILoggerFactory"/> instance used for logging.</param>
    public EthernetClient(ILoggerFactory loggerFactory)
        : base(loggerFactory.CreateLogger<EthernetClient>())
        => this.loggerFactory = loggerFactory;

    /// <inheritdoc/>
    public bool IsConnected => Socket != null && Socket.Connected;

    /// <inheritdoc/>
    public void ConnectTo(string ip, int port, ProtocolType protocolType)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            throw new ArgumentException("No valid ip address specified", nameof(ip));
        }

        if (port is <= 0 or > 65535)
        {
            throw new ArgumentException($"{port} is not a valid ip4 port number", nameof(port));
        }

        if (Socket != null)
        {
            Logger.LogDebug("Need to close the previous connection first before opening new one");
            Close();
        }

        IPEndPoint? endpoint = null;
        try
        {
            endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            Logger.LogInformation("Opening connection to {Endpoint}", endpoint);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
            _ = Socket.BeginConnect(endpoint, ConnectCallback, Socket);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to connect to {Endpoint}", endpoint);
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

            var remoteEndpoint = Socket.RemoteEndPoint;
            Logger.LogDebug("Closing connection");
            Socket.Disconnect(false);
            Socket.Close();
            Socket = null;
            Logger.LogInformation("{RemoteEndpoint} Connection closed", remoteEndpoint);
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

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Logger.LogTrace("Retrieving the socket from the state object");
            if (ar.AsyncState is not Socket client)
            {
                Logger.LogCritical("No socket was passed as state object!");
                return;
            }

            Logger.LogTrace("Complete the connection.");
            client.EndConnect(ar);
            Logger.LogInformation("Connected to: {RemoteEndpoint}", client.RemoteEndPoint);
            var ethernetConnection = new EthernetConnection(loggerFactory.CreateLogger<EthernetConnection>(), client);
            _ = ethernetConnection.SessionStream.Subscribe(ConnectionState);
        }
        catch (Exception ex)
        {
            Socket = null;
            Logger.LogError(ex, $"Connect failed");
        }
    }
}