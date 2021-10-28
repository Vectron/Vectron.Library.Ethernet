using System.Collections.Generic;
using System.Net.Sockets;

namespace VectronsLibrary.Ethernet;

/// <summary>
/// State object for receiving data from remote device.
/// </summary>
public sealed class StateObject
{
    /// <summary>
    /// Size of receive buffer.
    /// </summary>
    public const int BufferSize = 1024;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateObject"/> class.
    /// </summary>
    /// <param name="workSocket">The socket where data is coming from.</param>
    public StateObject(Socket workSocket)
    {
        WorkSocket = workSocket;
        RawBytes = new List<byte>(BufferSize);
    }

    /// <summary>
    /// Gets or sets the buffer for storing the raw bytes.
    /// </summary>
    public byte[] Buffer { get; set; } = new byte[BufferSize];

    /// <summary>
    /// Gets a <see cref="IList{T}"/> for storing all the <see langword="byte"/>.
    /// </summary>
    public IList<byte> RawBytes
    {
        get;
    }

    /// <summary>
    /// Gets the socket where data is coming from.
    /// </summary>
    public Socket WorkSocket
    {
        get;
    }
}