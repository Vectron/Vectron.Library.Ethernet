using System.Collections.Generic;
using System.Net.Sockets;

namespace VectronsLibrary.Ethernet
{
    // State object for receiving data from remote device.
    public sealed class StateObject
    {
        // Size of receive buffer.
        public const int BufferSize = 1024;

        public StateObject(Socket workSocket)
        {
            WorkSocket = workSocket;
            RawBytes = new List<byte>(BufferSize);
        }

        // Receive buffer.
        public byte[] Buffer { get; set; } = new byte[BufferSize];

        // Received data.
        public IList<byte> RawBytes
        {
            get;
        }

        // Client socket.
        public Socket WorkSocket
        {
            get;
        }
    }
}