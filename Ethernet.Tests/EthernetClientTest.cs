﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VectronsLibrary.Ethernet.Tests;

[TestClass]
public class EthernetClientTest : EthernetTestBase
{
    [TestMethod]
    public async Task ClientConnectTestAsync()
    {
        var localIp = GetLocalIPAddress();
        var ethernetServer = new EthernetServer(LoggerFactory);
        ethernetServer.Open(localIp, 100, System.Net.Sockets.ProtocolType.Tcp);

        var ethernetClient = new EthernetClient(LoggerFactory);
        ethernetClient.ConnectTo(localIp, 100, System.Net.Sockets.ProtocolType.Tcp);

        await Task.Delay(100);

        Assert.IsTrue(ethernetClient.IsConnected);
        Assert.IsTrue(ethernetServer.ListClients.Count() == 1);
    }

    [TestMethod]
    public void InvallidIpTest()
    {
        var ethernetClient = new EthernetClient(LoggerFactory);
        _ = Assert.ThrowsException<ArgumentException>(() => ethernetClient.ConnectTo(string.Empty, 200, System.Net.Sockets.ProtocolType.Tcp));
    }

    [TestMethod]
    public void InvallidPortTest()
    {
        var localIp = GetLocalIPAddress();
        var ethernetClient = new EthernetClient(LoggerFactory);
        _ = Assert.ThrowsException<ArgumentException>(() => ethernetClient.ConnectTo(localIp, -1, System.Net.Sockets.ProtocolType.Tcp));
    }

    [TestMethod]
    public async Task ReceiveDataTestAsync()
    {
        var localIp = GetLocalIPAddress();
        var testMessage = "this is a test message";
        var ethernetServer = new EthernetServer(LoggerFactory);
        ethernetServer.Open(localIp, 300, System.Net.Sockets.ProtocolType.Tcp);
        var subscription = ethernetServer.SessionStream.Where(x => x.IsConnected).Delay(TimeSpan.FromSeconds(1)).Subscribe(x => x.Value?.Send(testMessage));

        var ethernetClient = new EthernetClient(LoggerFactory);
        ethernetClient.ConnectTo(localIp, 300, System.Net.Sockets.ProtocolType.Tcp);
        var clientConnection = await ethernetClient.SessionStream.Where(x => x.IsConnected).Select(x => x.Value).FirstAsync();
        var first = await clientConnection.ReceivedDataStream.Timeout(TimeSpan.FromSeconds(2)).FirstAsync();

        Assert.AreEqual(testMessage, first.Message);
    }
}