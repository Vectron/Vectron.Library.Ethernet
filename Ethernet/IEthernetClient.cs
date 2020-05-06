using System.Net.Sockets;

namespace VectronsLibrary.Ethernet
{
    public interface IEthernetClient : IEthernet
    {
        bool IsConnected
        {
            get;
        }

        void ConnectTo(string ip, int port, ProtocolType protocolType);

        void Disconnect();

        void Send(string message);
    }
}