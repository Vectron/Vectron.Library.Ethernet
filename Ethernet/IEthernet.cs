using System;
using System.Net.Sockets;

namespace VectronsLibrary.Ethernet;

/// <summary>
/// A connection to cuimmunicate between clients over a ethernet socket.
/// </summary>
public interface IEthernet
{
    /// <summary>
    /// Gets the observable stream with connection updates from this connection.
    /// </summary>
    IObservable<IConnected<IEthernetConnection>> SessionStream
    {
        get;
    }

    /// <summary>
    /// Gets the underlying socket used for communication.
    /// </summary>
    Socket? Socket
    {
        get;
    }

    /// <summary>
    /// Close the connection.
    /// </summary>
    void Close();
}