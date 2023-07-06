using System.Net;
using Microsoft.Extensions.Logging;

namespace VectronsLibrary.Ethernet;

/// <summary>
/// Default implementation of <see cref="IEthernetClient"/>.
/// </summary>
public partial class EthernetClient
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Connected to: `{endPoint}`")]
    private partial void ConnectedTo(EndPoint endPoint);

    [LoggerMessage(EventId = 2, Level = LogLevel.Critical, Message = "Failed to opening connection to: `{IpAddress}:{Port}`")]
    private partial void FailedToConnect(string ipAddress, int port, Exception exception);

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Opening connection to: `{endPoint}`")]
    private partial void StartingToConnect(EndPoint endPoint);
}
