using Microsoft.Extensions.Options;

namespace VectronsLibrary.Ethernet;

/// <summary>
/// Set defaults values for the <see cref="EthernetClientOptions"/>.
/// </summary>
public class EthernetClientOptionsDefaults : IConfigureOptions<EthernetClientOptions>
{
    /// <inheritdoc/>
    public void Configure(EthernetClientOptions options)
    {
        options.IpAddress = "127.0.0.1";
        options.Port = 200;
        options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
    }
}
