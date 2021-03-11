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

        IEnumerable<IEthernetConnection> ListClients
        {
            get;
        }

        void Open(string ip, int port, ProtocolType protocolType);

        void Send(byte[] data);

        void Send(string message);
    }
}