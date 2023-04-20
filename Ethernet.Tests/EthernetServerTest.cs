using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VectronsLibrary.Ethernet.Tests;

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
    public void InvalidIpTest()
    {
        var ethernetServer = new EthernetServer(TestHelpers.LoggerFactory);
        _ = Assert.ThrowsException<ArgumentException>(() => ethernetServer.Open(string.Empty, 400, System.Net.Sockets.ProtocolType.Tcp));
    }

    /// <summary>
    /// Test if we get an exception when no valid port is given.
    /// </summary>
    [TestMethod]
    public void InvalidPortTest()
    {
        var localIp = TestHelpers.GetLocalIPAddress();
        var ethernetServer = new EthernetServer(TestHelpers.LoggerFactory);
        _ = Assert.ThrowsException<ArgumentException>(() => ethernetServer.Open(localIp, -1, System.Net.Sockets.ProtocolType.Tcp));
    }

    /// <summary>
    /// Test if we can open a server on the local system.
    /// </summary>
    [TestMethod]
    public void ServerCreationTest()
    {
        var localIp = TestHelpers.GetLocalIPAddress();
        var ethernetServer = new EthernetServer(TestHelpers.LoggerFactory);
        ethernetServer.Open(localIp, 500, System.Net.Sockets.ProtocolType.Tcp);
        Assert.IsTrue(ethernetServer.IsOnline);
        ethernetServer.Dispose();
    }
}