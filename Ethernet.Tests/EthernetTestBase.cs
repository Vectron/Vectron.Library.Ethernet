using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace VectronsLibrary.Ethernet.Tests.NetFramework
{
    public abstract class EthernetTestBase
    {
        protected static readonly ILoggerFactory LoggerFactory;

        static EthernetTestBase()
        {
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(x => x.AddDebug().AddConsole().SetMinimumLevel(LogLevel.Trace));
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new NotSupportedException("No network adapters with an IPv4 address in the system!");
        }
    }
}