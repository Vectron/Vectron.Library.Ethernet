using System;

namespace VectronsLibrary.Ethernet
{
    /// <summary>
    /// Represents a connection that can be used to send messages.
    /// </summary>
    public interface IEthernetConnection : IEthernet
    {
        /// <summary>
        /// Gets a value indicating whether if a connection is open.
        /// </summary>
        bool IsConnected
        {
            get;
        }

        /// <summary>
        /// Gets the stream with decoded received data.
        /// </summary>
        IObservable<ReceivedData> ReceivedDataStream
        {
            get;
        }

        /// <summary>
        /// Send raw bytes.
        /// </summary>
        /// <param name="data">The data to send.</param>
        void Send(byte[] data);

        /// <summary>
        /// Sends a string encoded as ascii.
        /// </summary>
        /// <param name="data">The string to send.</param>
        void Send(string data);
    }
}