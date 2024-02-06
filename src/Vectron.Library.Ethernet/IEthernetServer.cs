using System.Net;
using System.Net.Sockets;

namespace Vectron.Library.Ethernet;

/// <summary>
/// An implementation for a ethernet server.
/// </summary>
public interface IEthernetServer
{
    /// <summary>
    /// Gets a <see cref="IEnumerable{T}"/> of all connected clients.
    /// </summary>
    IEnumerable<IEthernetConnection> Clients
    {
        get;
    }

    /// <summary>
    /// Gets a stream with updates from the client connection state.
    /// </summary>
    IObservable<IConnected<IEthernetConnection>> ConnectionStream
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether the server is listening for connections.
    /// </summary>
    bool IsListening
    {
        get;
    }

    /// <summary>
    /// Send data to all clients.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task BroadCastAsync(ReadOnlyMemory<byte> data);

    /// <summary>
    /// Send a message to all clients.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task BroadCastAsync(string message);

    /// <summary>
    /// Close the server.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CloseAsync();

    /// <summary>
    /// Start listening for connections.
    /// </summary>
    /// <param name="endPoint">The <see cref="IPEndPoint"/> to listen on.</param>
    /// <param name="protocolType">The <see cref="ProtocolType"/> to use.</param>
    void Open(IPEndPoint endPoint, ProtocolType protocolType);

    /// <summary>
    /// Start listening for connections.
    /// </summary>
    void Open();
}
