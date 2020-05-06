using System;
using System.Net.Sockets;

namespace VectronsLibrary.Ethernet
{
    public interface IEthernet : IDisposable
    {
        IObservable<ReceivedData> ReceivedDataStream
        {
            get;
        }

        IObservable<IConnected<Socket>> SessionStream
        {
            get;
        }

        void Send(Socket handler, string data);
    }
}