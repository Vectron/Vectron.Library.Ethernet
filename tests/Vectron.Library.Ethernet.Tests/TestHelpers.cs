using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace Vectron.Library.Ethernet.Tests;

/// <summary>
/// Helper methods for running tests.
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Create and configure an <see cref="IOptions{TOptions}"/>.
    /// </summary>
    /// <typeparam name="T">The option type to create.</typeparam>
    /// <param name="configure">Action for configuring the option.</param>
    /// <returns>The created <see cref="IOptions{TOptions}"/> instance.</returns>
    [ExcludeFromCodeCoverage]
    public static IOptions<T> CreateOptions<T>(Action<T> configure)
        where T : class, new()
    {
        var instance = new T();
        configure(instance);
        return Options.Create(instance);
    }

    /// <summary>
    /// Function for getting the local ip-address of the system.
    /// </summary>
    /// <returns>The ip-address string.</returns>
    /// <exception cref="NotSupportedException">
    /// When no network adapters are found with an IP4 address.
    /// </exception>
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
