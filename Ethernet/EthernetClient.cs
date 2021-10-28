using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace VectronsLibrary.Ethernet;

/// <summary>
/// Default implementation of <see cref="IEthernetClient"/>.
/// </summary>
public sealed class EthernetClient : EthernetConnection, IEthernetClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetClient"/> class.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger"/> instance used for logging.</param>
    public EthernetClient(ILogger<EthernetClient> logger)
        : base(logger)
    {
    }

    /// <inheritdoc/>
    public void ConnectTo(string ip, int port, ProtocolType protocolType)
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
            ConnectionState.OnNext(Connected.Yes(this));
            var state = new StateObject(client);
            Logger.LogDebug("Start listening for new messages");
            _ = client.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
        }
        catch (Exception ex)
        {
            Socket = null;
            Logger.LogError(ex, $"Connect failed");
            ConnectionState.OnNext(Connected.No(this));
        }
    }
}