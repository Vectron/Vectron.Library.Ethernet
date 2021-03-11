using System;
using System.Net.Sockets;

namespace VectronsLibrary.Ethernet
{
    public interface IEthernet
    {
        IObservable<IConnected<IEthernetConnection>> SessionStream
        {
            get;
        }

        Socket Socket
        {
            get;
        }

        void Close();
    }
}