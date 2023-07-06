using Microsoft.Extensions.Options;

namespace VectronsLibrary.Ethernet;

/// <summary>
/// Set defaults values for the <see cref="EthernetServerOptions"/>.
/// </summary>
public class EthernetServerOptionsDefaults : IConfigureOptions<EthernetServerOptions>
{
    /// <inheritdoc/>
    public void Configure(EthernetServerOptions options)
    {
        options.IpAddress = "127.0.0.1";
        options.Port = 200;
        options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
    }
}
