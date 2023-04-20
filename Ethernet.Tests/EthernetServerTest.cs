using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VectronsLibrary.Ethernet.Tests;

/// <summary>
/// A test class for testing the <see cref="EthernetServer"/>.
/// </summary>
[TestClass]
public class EthernetServerTest : EthernetTestBase
{
    /// <summary>
    /// Test if we get an exception when no valid ip-address is given.
    /// </summary>
    [TestMethod]
    public void InvallidIpTest()
    {
        var ethernetServer = new EthernetServer(LoggerFactory);
        _ = Assert.ThrowsException<ArgumentException>(() => ethernetServer.Open(string.Empty, 400, System.Net.Sockets.ProtocolType.Tcp));
    }

    /// <summary>
    /// Test if we get an exception when no valid port is given.
    /// </summary>
    [TestMethod]
    public void InvallidPortTest()
    {
        var localIp = GetLocalIPAddress();
        var ethernetServer = new EthernetServer(LoggerFactory);
        _ = Assert.ThrowsException<ArgumentException>(() => ethernetServer.Open(localIp, -1, System.Net.Sockets.ProtocolType.Tcp));
    }

    /// <summary>
    /// Test if we can open a server on the local system.
    /// </summary>
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