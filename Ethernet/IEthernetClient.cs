using System.Net.Sockets;

namespace VectronsLibrary.Ethernet
{
    public interface IEthernetClient : IEthernetConnection
    {
        void ConnectTo(string ip, int port, ProtocolType protocolType);
    }
}