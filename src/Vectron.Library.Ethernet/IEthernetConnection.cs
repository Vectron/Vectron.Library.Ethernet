namespace Vectron.Library.Ethernet;

/// <summary>
/// Represents a connection that can be used to send messages.
/// </summary>
public interface IEthernetConnection
{
    /// <summary>
    /// Events is fired when the connection is closed.
    /// </summary>
    event EventHandler? ConnectionClosed;

    /// <summary>
    /// Gets a value indicating whether if a connection is open.
    /// </summary>
    bool IsConnected
    {
        get;
    }

    /// <summary>
    /// Gets the stream with decoded received data.
    /// </summary>
    IObservable<ReceivedData> ReceivedDataStream
    {
        get;
    }

    /// <summary>
    /// Close this connection.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CloseAsync();

    /// <summary>
    /// Send raw bytes.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SendAsync(ReadOnlyMemory<byte> data);

    /// <summary>
    /// Sends a string encoded as ascii.
    /// </summary>
    /// <param name="message">The string to send.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SendAsync(string message);
}
