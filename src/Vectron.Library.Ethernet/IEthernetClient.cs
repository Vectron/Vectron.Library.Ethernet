namespace VectronsLibrary.Ethernet;

/// <summary>
/// An ethernet client that can be used to connect to a server.
/// </summary>
public interface IEthernetClient : IEthernetConnection
{
    /// <summary>
    /// Try to open a connection to the given IP and port.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
}
