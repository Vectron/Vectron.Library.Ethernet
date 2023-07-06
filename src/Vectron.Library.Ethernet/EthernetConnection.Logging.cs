using System.Net;
using Microsoft.Extensions.Logging;

namespace Vectron.Library.Ethernet;

/// <summary>
/// Implementation of <see cref="IEthernetConnection"/>.
/// </summary>
public partial class EthernetConnection
{
    [LoggerMessage(EventId = 4, Level = LogLevel.Critical, Message = "Failed sending data to `{Endpoint}`")]
    private partial void FailedToSend(EndPoint? endpoint, Exception exception);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Received: {ReceivedData} - From: {Endpoint}")]
    private partial void MessageReceived(ReceivedData receivedData, EndPoint? endpoint);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Sending: {ByteCount} bytes - To: {Endpoint}")]
    private partial void MessageSending(int byteCount, EndPoint? endpoint);

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Connections closed by: {Endpoint}")]
    private partial void RemoteRequestedClose(EndPoint? endpoint);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Connections closed: {Endpoint}")]
    private partial void RequestedClose(EndPoint? endpoint);

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Started listening for new messages")]
    private partial void StartReceivingData();
}
