using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace VectronsLibrary.Ethernet.Tests.NetFramework
{
    public abstract class EthernetTestBase
    {
        [ExcludeFromCodeCoverage]
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