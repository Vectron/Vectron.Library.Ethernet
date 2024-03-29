using System.Net;
using System.Net.Sockets;

namespace Vectron.Library.Ethernet;

/// <summary>
/// An ethernet client that can be used to connect to a server.
/// </summary>
public interface IEthernetClient : IEthernetConnection
{
    /// <summary>
    /// Try to open a connection using the configured options.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Try to open a connection to the given IP and port.
    /// </summary>
    /// <param name="endpoint">The <see cref="IPEndPoint"/> to connect to.</param>
    /// <param name="protocolType">The protocol type to use.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<bool> ConnectAsync(IPEndPoint endpoint, ProtocolType protocolType, CancellationToken cancellationToken = default);
}
