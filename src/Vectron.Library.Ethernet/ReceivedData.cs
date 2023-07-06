using System.Text;

namespace VectronsLibrary.Ethernet;

/// <summary>
/// A full received message.
/// </summary>
public sealed record ReceivedData(byte[] RawData)
{
    /// <summary>
    /// Gets the data as a ASCII string.
    /// </summary>
    public string Message
        => Encoding.ASCII.GetString(RawData, 0, RawData.Length);
}
