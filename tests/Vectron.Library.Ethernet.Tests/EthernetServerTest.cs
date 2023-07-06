using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vectron.Library.Ethernet.Tests;

/// <summary>
/// A test class for testing the <see cref="EthernetServer"/>.
/// </summary>
[TestClass]
public class EthernetServerTest
{
    /// <summary>
    /// Test if we get an exception when no valid ip-address is given.
    /// </summary>
    [TestMethod]
    public void InvalidIpTestAsync()
    {
        var serverSettings = TestHelpers.CreateOptions<EthernetServerOptions>(options =>
        {
            options.IpAddress = string.Empty;
            options.Port = 2100;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });

        using var ethernetServer = new EthernetServer(serverSettings, NullLogger<EthernetServer>.Instance);
        _ = Assert.ThrowsException<FormatException>(ethernetServer.Open);
        Assert.IsFalse(ethernetServer.IsListening, "Server not listening");
    }

    /// <summary>
    /// Test if we get an exception when no valid port is given.
    /// </summary>
    [TestMethod]
    public void InvalidPortTest()
    {
        var serverSettings = TestHelpers.CreateOptions<EthernetServerOptions>(options =>
        {
            options.IpAddress = TestHelpers.GetLocalIPAddress();
            options.Port = -1;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });

        using var ethernetServer = new EthernetServer(serverSettings, NullLogger<EthernetServer>.Instance);
        _ = Assert.ThrowsException<ArgumentOutOfRangeException>(ethernetServer.Open);
        Assert.IsFalse(ethernetServer.IsListening, "Server not listening");
    }

    /// <summary>
    /// Test if we can open a server on the local system.
    /// </summary>
    [TestMethod]
    public void ServerCreationTest()
    {
        var serverSettings = TestHelpers.CreateOptions<EthernetServerOptions>(options =>
        {
            options.IpAddress = TestHelpers.GetLocalIPAddress();
            options.Port = 2101;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });
        using var ethernetServer = new EthernetServer(serverSettings, NullLogger<EthernetServer>.Instance);
        ethernetServer.Open();
        Assert.IsTrue(ethernetServer.IsListening, "Server is listening");
    }
}
