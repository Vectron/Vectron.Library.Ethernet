using System.Globalization;
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

    [TestMethod]
    public async Task ServerCanBeOpenedAndClosedMultipleTimesAsync()
    {
        var serverSettings = TestHelpers.CreateOptions<EthernetServerOptions>(options =>
        {
            options.IpAddress = TestHelpers.GetLocalIPAddress();
            options.Port = 2102;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });
        using var ethernetServer = new EthernetServer(serverSettings, NullLogger<EthernetServer>.Instance);

        for (var i = 0; i < 3; i++)
        {
            ethernetServer.Open();
            Assert.IsTrue(ethernetServer.IsListening, $"Server is not listening; iteration:  {i.ToString(CultureInfo.InvariantCulture)}");
            await ethernetServer.CloseAsync();
            Assert.IsFalse(ethernetServer.IsListening, $"Server is still listening; iteration:  {i.ToString(CultureInfo.InvariantCulture)}");
        }
    }

    [TestMethod]
    public async Task ServerClosingDisconnectsClientsAsync()
    {
        var localIp = TestHelpers.GetLocalIPAddress();
        var serverSettings = TestHelpers.CreateOptions<EthernetServerOptions>(options =>
        {
            options.IpAddress = localIp;
            options.Port = 2103;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });

        using var ethernetServer = new EthernetServer(serverSettings, NullLogger<EthernetServer>.Instance);
        var clientSettings = TestHelpers.CreateOptions<EthernetClientOptions>(options =>
        {
            options.IpAddress = localIp;
            options.Port = 2103;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });
        using var ethernetClient1 = new EthernetClient(clientSettings, NullLogger<EthernetClient>.Instance);
        using var ethernetClient2 = new EthernetClient(clientSettings, NullLogger<EthernetClient>.Instance);
        using var ethernetClient3 = new EthernetClient(clientSettings, NullLogger<EthernetClient>.Instance);

        ethernetServer.Open();
        Assert.IsTrue(ethernetServer.IsListening, "Server is not listening");
        _ = await ethernetClient1.ConnectAsync();
        _ = await ethernetClient2.ConnectAsync();
        _ = await ethernetClient3.ConnectAsync();

        await TestHelpers.WaitForPredicate(() => ethernetClient1.IsConnected, TimeSpan.FromSeconds(1), "Client 1 not connected");
        await TestHelpers.WaitForPredicate(() => ethernetClient2.IsConnected, TimeSpan.FromSeconds(1), "Client 2 not connected");
        await TestHelpers.WaitForPredicate(() => ethernetClient3.IsConnected, TimeSpan.FromSeconds(1), "Client 3 not connected");

        await TestHelpers.WaitForPredicate(() => ethernetServer.Clients.Take(4).Count() == 3, TimeSpan.FromSeconds(1), "Not all clients connected");

        await ethernetServer.CloseAsync();
        Assert.IsFalse(ethernetServer.IsListening, "Server is still listening");
        await TestHelpers.WaitForPredicate(() => !ethernetClient1.IsConnected, TimeSpan.FromSeconds(1), "Client 1 not disconnected");
        await TestHelpers.WaitForPredicate(() => !ethernetClient2.IsConnected, TimeSpan.FromSeconds(1), "Client 2 not disconnected");
        await TestHelpers.WaitForPredicate(() => !ethernetClient3.IsConnected, TimeSpan.FromSeconds(1), "Client 3 not disconnected");
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
