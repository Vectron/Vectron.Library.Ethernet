using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
    public static IOptionsSnapshot<T> CreateOptions<T>(Action<T> configure)
        where T : class, new()
    {
        var instance = new T();
        configure(instance);

        var optionsMonitorMock = new Mock<IOptionsSnapshot<T>>();
        optionsMonitorMock.Setup(o => o.Value).Returns(instance);
        optionsMonitorMock.Setup(o => o.Get(It.IsAny<string>())).Returns(instance);
        return optionsMonitorMock.Object;
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
        var address = Dns.GetHostAddresses(string.Empty, AddressFamily.InterNetwork).FirstOrDefault()
            ?? throw new NotSupportedException("No network adapters with an IPv4 address in the system!");
        return address.ToString();
    }

    public static async Task WaitForPredicate(Func<bool> predicate, TimeSpan timeout, string timeoutMessage)
    {
        using var cts = new CancellationTokenSource(timeout);
        while (!predicate())
        {
            Assert.IsFalse(cts.Token.IsCancellationRequested, timeoutMessage);
            await Task.Delay(10, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
