using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace VectronsLibrary.Ethernet.Tests;

[TestClass]
public class EthernetClientTest : EthernetTestBase
{
    [TestMethod]
    public async Task ClientConnectTestAsync()
    {
        var localIp = GetLocalIPAddress();
        var ethernetServer = new EthernetServer(Mock.Of<ILogger<EthernetServer>>(), Mock.Of<ILogger<EthernetConnection>>());
        ethernetServer.Open(localIp, 100, System.Net.Sockets.ProtocolType.Tcp);

        var ethernetClient = new EthernetClient(Mock.Of<ILogger<EthernetClient>>());
        ethernetClient.ConnectTo(localIp, 100, System.Net.Sockets.ProtocolType.Tcp);

        await Task.Delay(100);

        Assert.IsTrue(ethernetClient.IsConnected);
        Assert.IsTrue(ethernetServer.ListClients.Count() == 1);
    }

    [TestMethod]
    public void InvallidIpTest()
    {
        var ethernetClient = new EthernetClient(Mock.Of<ILogger<EthernetClient>>());
        _ = Assert.ThrowsException<ArgumentException>(() => ethernetClient.ConnectTo(string.Empty, 200, System.Net.Sockets.ProtocolType.Tcp));
    }

    [TestMethod]
    public void InvallidPortTest()
    {
        var localIp = GetLocalIPAddress();
        var ethernetClient = new EthernetClient(Mock.Of<ILogger<EthernetClient>>());
        _ = Assert.ThrowsException<ArgumentException>(() => ethernetClient.ConnectTo(localIp, -1, System.Net.Sockets.ProtocolType.Tcp));
    }

    [TestMethod]
    public async Task ReceiveDataTestAsync()
    {
        var localIp = GetLocalIPAddress();
        var testMessage = "this is a test message";
        var ethernetServer = new EthernetServer(Mock.Of<ILogger<EthernetServer>>(), Mock.Of<ILogger<EthernetConnection>>());
        ethernetServer.Open(localIp, 300, System.Net.Sockets.ProtocolType.Tcp);
        var subscription = ethernetServer.SessionStream.Where(x => x.IsConnected).Delay(TimeSpan.FromSeconds(1)).Subscribe(x => x.Value?.Send(testMessage));

        var ethernetClient = new EthernetClient(Mock.Of<ILogger<EthernetClient>>());
        ethernetClient.ConnectTo(localIp, 300, System.Net.Sockets.ProtocolType.Tcp);
        var first = await ethernetClient.ReceivedDataStream.Timeout(TimeSpan.FromSeconds(2)).FirstAsync();

        Assert.AreEqual(testMessage, first.Message);
    }
}