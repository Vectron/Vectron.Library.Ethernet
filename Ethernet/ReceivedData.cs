using System.Net.Sockets;
using System.Text;

namespace VectronsLibrary.Ethernet
{
    public sealed class ReceivedData
    {
        public ReceivedData(byte[] rawData, Socket socket)
        {
            RawData = rawData;
            Sender = socket;
        }

        public string Message
            => Encoding.ASCII.GetString(RawData, 0, RawData.Length);

        public byte[] RawData
        {
            get;
        }

        public Socket Sender
        {
            get;
        }
    }
}