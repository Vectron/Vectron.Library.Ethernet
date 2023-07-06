using System.Net;
using Microsoft.Extensions.Logging;

namespace Vectron.Library.Ethernet;

/// <summary>
/// An Ethernet server implementation.
/// </summary>
public partial class EthernetServer
{
    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "New client connected from: {EndPoint}")]
    private partial void ClientConnected(EndPoint? endPoint);

    [LoggerMessage(EventId = 2, Level = LogLevel.Critical, Message = "Failed to Listen on: {IpAddress}:{port}")]
    private partial void FailedToListen(string ipAddress, int port, Exception exception);

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Started listening on: {EndPoint}")]
    private partial void StartListening(EndPoint? endPoint);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Stopped listening on: {EndPoint}")]
    private partial void StoppedListening(EndPoint? endPoint);
}
