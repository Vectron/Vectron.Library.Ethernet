using System;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace VectronsLibrary.Ethernet.Tests.NetFramework
{
    [TestClass]
    public class EthernetServerTest : EthernetTestBase
    {
        [TestMethod]
        public void InvallidIpTest()
        {
            var ethernetServer = new EthernetServer(Mock.Of<ILogger<EthernetServer>>(), Mock.Of<ILogger<EthernetConnection>>());
            _ = Assert.ThrowsException<ArgumentException>(() => ethernetServer.Open(string.Empty, 400, System.Net.Sockets.ProtocolType.Tcp));
        }

        [TestMethod]
        public void InvallidPortTest()
        {
            var localIp = GetLocalIPAddress();
            var ethernetServer = new EthernetServer(Mock.Of<ILogger<EthernetServer>>(), Mock.Of<ILogger<EthernetConnection>>());
            _ = Assert.ThrowsException<ArgumentException>(() => ethernetServer.Open(localIp, -1, System.Net.Sockets.ProtocolType.Tcp));
        }

        [TestMethod]
        public void ServerCreationTest()
        {
            var localIp = GetLocalIPAddress();
            var ethernetServer = new EthernetServer(Mock.Of<ILogger<EthernetServer>>(), Mock.Of<ILogger<EthernetConnection>>());
            ethernetServer.Open(localIp, 500, System.Net.Sockets.ProtocolType.Tcp);
            Assert.IsTrue(ethernetServer.IsOnline);
            ethernetServer.Dispose();
        }
    }
}