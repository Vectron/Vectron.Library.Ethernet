using System.Collections.Generic;
using System.Net.Sockets;

namespace VectronsLibrary.Ethernet
{
    /// <summary>
    /// An ethernet server.
    /// </summary>
    public interface IEthernetServer : IEthernet
    {
        /// <summary>
        /// Gets a value indicating whether the server is online.
        /// </summary>
        bool IsOnline
        {
            get;
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{IEthernetConnection}"/> with all curently connected clients.
        /// </summary>
        IEnumerable<IEthernetConnection> ListClients
        {
            get;
        }

        /// <summary>
        /// Start listening for connections.
        /// </summary>
        /// <param name="ip">The ip adress to listen on.</param>
        /// <param name="port">The port number to listen on.</param>
        /// <param name="protocolType">The protocol to listen with.</param>
        void Open(string ip, int port, ProtocolType protocolType);

        /// <summary>
        /// Send raw bytes to all connected clients.
        /// </summary>
        /// <param name="data">The data to send.</param>
        void Send(byte[] data);

        /// <summary>
        /// Sends a string encoded as ascii to all connected clients.
        /// </summary>
        /// <param name="message">The string to send.</param>
        void Send(string message);
    }
}