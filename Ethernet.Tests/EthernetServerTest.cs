using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VectronsLibrary.Ethernet.Tests;

[TestClass]
public class EthernetServerTest : EthernetTestBase
{
    [TestMethod]
    public void InvallidIpTest()
    {
        var ethernetServer = new EthernetServer(LoggerFactory);
        _ = Assert.ThrowsException<ArgumentException>(() => ethernetServer.Open(string.Empty, 400, System.Net.Sockets.ProtocolType.Tcp));
    }

    [TestMethod]
    public void InvallidPortTest()
    {
        var localIp = GetLocalIPAddress();
        var ethernetServer = new EthernetServer(LoggerFactory);
        _ = Assert.ThrowsException<ArgumentException>(() => ethernetServer.Open(localIp, -1, System.Net.Sockets.ProtocolType.Tcp));
    }

    [TestMethod]
    public void ServerCreationTest()
    {
        var localIp = GetLocalIPAddress();
        var ethernetServer = new EthernetServer(LoggerFactory);
        ethernetServer.Open(localIp, 500, System.Net.Sockets.ProtocolType.Tcp);
        Assert.IsTrue(ethernetServer.IsOnline);
        ethernetServer.Dispose();
    }
}