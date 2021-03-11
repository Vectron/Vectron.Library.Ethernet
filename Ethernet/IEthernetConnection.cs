using System;

namespace VectronsLibrary.Ethernet
{
    public interface IEthernetConnection : IEthernet
    {
        bool IsConnected
        {
            get;
        }

        IObservable<ReceivedData> ReceivedDataStream
        {
            get;
        }

        void Send(byte[] data);

        void Send(string data);
    }
}