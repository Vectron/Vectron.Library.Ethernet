using System.Net.Sockets;
using System.Text;

namespace VectronsLibrary.Ethernet;

/// <summary>
/// A full received message.
/// </summary>
public sealed class ReceivedData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivedData"/> class.
    /// </summary>
    /// <param name="rawData">The raw received data.</param>
    /// <param name="socket">The originating socket.</param>
    public ReceivedData(byte[] rawData, Socket socket)
    {
        RawData = rawData;
        Sender = socket;
    }

    /// <summary>
    /// Gets the data as a ASCII string.
    /// </summary>
    public string Message
        => Encoding.ASCII.GetString(RawData, 0, RawData.Length);

    /// <summary>
    /// Gets the raw received data.
    /// </summary>
    public byte[] RawData
    {
        get;
    }

    /// <summary>
    /// Gets the raw socket to connect to the sender of the data.
    /// </summary>
    public Socket Sender
    {
        get;
    }
}