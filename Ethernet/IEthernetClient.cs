using System.Net.Sockets;

namespace VectronsLibrary.Ethernet;

/// <summary>
/// An ethernet client that can be used to connect to a server.
/// </summary>
public interface IEthernetClient : IEthernetConnection
{
    /// <summary>
    /// Try to open a connection to the given IP and port.
    /// </summary>
    /// <param name="ip">The ip adress to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    /// <param name="protocolType">The protocol thype to connect with.</param>
    void ConnectTo(string ip, int port, ProtocolType protocolType);
}