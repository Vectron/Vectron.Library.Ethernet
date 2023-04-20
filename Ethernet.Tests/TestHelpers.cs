using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace VectronsLibrary.Ethernet.Tests;

/// <summary>
/// Helper methods for running tests.
/// </summary>
internal sealed class TestHelpers
{
    /// <summary>
    /// Gets a <see cref="ILoggerFactory"/> mock.
    /// </summary>
    public static ILoggerFactory LoggerFactory
    {
        get
        {
            var loggerFactoryMock = Mock.Of<ILoggerFactory>();
            _ = Mock.Get(loggerFactoryMock).Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(NullLogger.Instance);
            return loggerFactoryMock;
        }
    }

    /// <summary>
    /// Function for getting the local ip-address of the system.
    /// </summary>
    /// <returns>The ip-address string.</returns>
    /// <exception cref="NotSupportedException">When no network adapters are found with an IP4 address.</exception>
    [ExcludeFromCodeCoverage]
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new NotSupportedException("No network adapters with an IPv4 address in the system!");
    }
}