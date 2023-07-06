using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace Vectron.Library.Ethernet;

/// <summary>
/// Settings shared by the <see cref="EthernetClient"/> and <see cref="EthernetServer"/>.
/// </summary>
public class EthernetOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EthernetOptions"/> class.
    /// </summary>
    public EthernetOptions()
        => IpAddress = string.Empty;

    /// <summary>
    /// Gets or sets the Ip address to connect to.
    /// </summary>
    [Required]
    [RegularExpression("^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.?\\b){4}$")]
    public string IpAddress
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the port to connect to.
    /// </summary>
    [Required]
    [Range(1, 65535, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public int Port
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the protocol to use when communicating.
    /// </summary>
    [Required]
    public ProtocolType ProtocolType
    {
        get;
        set;
    }
}
