using System.Collections.Generic;
using System.Net.Sockets;

namespace VectronsLibrary.Ethernet
{
    public interface IEthernetServer : IEthernet
    {
        bool IsOnline
        {
            get;
        }

        List<Socket> ListClients
        {
            get;
        }

        void Close();

        void Open(string ip, int port, ProtocolType protocolType);

        void Send(string message);
    }
}