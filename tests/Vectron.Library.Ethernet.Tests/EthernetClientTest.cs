using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vectron.Library.Ethernet.Tests;

/// <summary>
/// A test class for testing the <see cref="EthernetClient"/>.
/// </summary>
[TestClass]
public class EthernetClientTest
{
    /// <summary>
    /// Test if the client connects to the server.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents this test.</returns>
    [TestMethod]
    public async Task ClientConnectTestAsync()
    {
        var localIp = TestHelpers.GetLocalIPAddress();
        var serverSettings = TestHelpers.CreateOptions<EthernetServerOptions>(options =>
        {
            options.IpAddress = localIp;
            options.Port = 2000;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });
        using var ethernetServer = new EthernetServer(serverSettings, NullLogger<EthernetServer>.Instance);

        var clientSettings = TestHelpers.CreateOptions<EthernetClientOptions>(options =>
        {
            options.IpAddress = localIp;
            options.Port = 2000;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });
        using var ethernetClient = new EthernetClient(clientSettings, NullLogger<EthernetClient>.Instance);

        ethernetServer.Open();
        _ = await ethernetClient.ConnectAsync();

        Assert.IsTrue(ethernetClient.IsConnected, "Client connected");
        await Task.Delay(10);
        Assert.IsTrue(ethernetServer.Clients.Take(2).Count() == 1, "Client count is not 1");
        await ethernetClient.CloseAsync();
        Assert.IsFalse(ethernetClient.IsConnected, "Client still connected connected");
        await Task.Delay(10);
        Assert.IsTrue(ethernetServer.Clients.Any(), "Client count is not 0");
    }

    /// <summary>
    /// Test if we get an exception when no valid ip-address is given.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task InvalidIpTestAsync()
    {
        var clientSettings = TestHelpers.CreateOptions<EthernetClientOptions>(options =>
        {
            options.IpAddress = string.Empty;
            options.Port = 2001;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });

        using var ethernetClient = new EthernetClient(clientSettings, NullLogger<EthernetClient>.Instance);
        var result = await ethernetClient.ConnectAsync();
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Test if we get an exception when no valid port is given.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task InvalidPortTestAsync()
    {
        var clientSettings = TestHelpers.CreateOptions<EthernetClientOptions>(options =>
        {
            options.IpAddress = TestHelpers.GetLocalIPAddress();
            options.Port = -1;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });

        using var ethernetClient = new EthernetClient(clientSettings, NullLogger<EthernetClient>.Instance);
        var result = await ethernetClient.ConnectAsync();
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Test if the client can receive data from the server.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents this test.</returns>
    [TestMethod]
    public async Task ReceiveDataTestAsync()
    {
        var localIp = TestHelpers.GetLocalIPAddress();
        var testMessage = "this is a test message";
        var serverSettings = TestHelpers.CreateOptions<EthernetServerOptions>(options =>
        {
            options.IpAddress = localIp;
            options.Port = 2002;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });

        using var ethernetServer = new EthernetServer(serverSettings, NullLogger<EthernetServer>.Instance);
        var clientSettings = TestHelpers.CreateOptions<EthernetClientOptions>(options =>
        {
            options.IpAddress = localIp;
            options.Port = 2002;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });

        using var ethernetClient = new EthernetClient(clientSettings, NullLogger<EthernetClient>.Instance);
        ethernetServer.Open();
        _ = await ethernetClient.ConnectAsync();

        var task1 = ethernetClient.ReceivedDataStream.Timeout(TimeSpan.FromSeconds(2)).FirstAsync().ToTask();
        await Task.Delay(10);
        await ethernetServer.BroadCastAsync(testMessage);
        var results = await task1;

        Assert.AreEqual(testMessage, results.Message);
    }

    /// <summary>
    /// Test if multiple data listeners can be added.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task SubscribingMultipleTimesDoesNotThrowErrorsAsync()
    {
        var localIp = TestHelpers.GetLocalIPAddress();
        var testMessage = "this is a test message";
        var serverSettings = TestHelpers.CreateOptions<EthernetServerOptions>(options =>
        {
            options.IpAddress = localIp;
            options.Port = 2003;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });

        using var ethernetServer = new EthernetServer(serverSettings, NullLogger<EthernetServer>.Instance);
        var clientSettings = TestHelpers.CreateOptions<EthernetClientOptions>(options =>
        {
            options.IpAddress = localIp;
            options.Port = 2003;
            options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
        });

        using var ethernetClient = new EthernetClient(clientSettings, NullLogger<EthernetClient>.Instance);

        ethernetServer.Open();
        _ = await ethernetClient.ConnectAsync();

        var task1 = ethernetClient.ReceivedDataStream.Timeout(TimeSpan.FromSeconds(2)).FirstAsync().ToTask();
        var task2 = ethernetClient.ReceivedDataStream.Timeout(TimeSpan.FromSeconds(2)).FirstAsync().ToTask();
        var task3 = ethernetClient.ReceivedDataStream.Timeout(TimeSpan.FromSeconds(2)).FirstAsync().ToTask();
        var task4 = ethernetClient.ReceivedDataStream.Timeout(TimeSpan.FromSeconds(2)).FirstAsync().ToTask();

        await Task.Delay(10);
        await ethernetServer.BroadCastAsync(testMessage);
        var results = await Task.WhenAll(task1, task2, task3, task4);

        Assert.AreEqual(testMessage, results[0].Message);
        Assert.AreEqual(testMessage, results[1].Message);
        Assert.AreEqual(testMessage, results[2].Message);
        Assert.AreEqual(testMessage, results[3].Message);
    }
}
