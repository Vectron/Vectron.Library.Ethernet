using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace VectronsLibrary.Ethernet.Tests.NetFramework
{
    [TestClass]
    public class EthernetServerTest : EthernetTestBase
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvallidIpTest()
        {
            var ethernetServer = new EthernetServer(loggerFactory.CreateLogger<EthernetServer>(), loggerFactory.CreateLogger<EthernetConnection>());
            ethernetServer.Open("", 400, System.Net.Sockets.ProtocolType.Tcp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvallidPortTest()
        {
            var localIp = GetLocalIPAddress();
            var ethernetServer = new EthernetServer(loggerFactory.CreateLogger<EthernetServer>(), loggerFactory.CreateLogger<EthernetConnection>());
            ethernetServer.Open(localIp, -1, System.Net.Sockets.ProtocolType.Tcp);
        }

        [TestMethod]
        public void ServerCreationTest()
        {
            var localIp = GetLocalIPAddress();
            var ethernetServer = new EthernetServer(loggerFactory.CreateLogger<EthernetServer>(), loggerFactory.CreateLogger<EthernetConnection>());
            ethernetServer.Open(localIp, 500, System.Net.Sockets.ProtocolType.Tcp);
            Assert.IsTrue(ethernetServer.IsOnline);
            ethernetServer.Dispose();
        }
    }
}