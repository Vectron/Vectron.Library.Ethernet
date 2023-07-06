using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VectronsLibrary.Ethernet;
using VectronsLibrary.Ethernet.Sandbox;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "hh:mm:ss:fff";
});

builder.Services.AddEthernetServer(options =>
{
    options.IpAddress = "127.0.0.1";
    options.Port = 200;
    options.ProtocolType = System.Net.Sockets.ProtocolType.Tcp;
});

builder.Services.AddHostedService<EthernetHost>();

using var host = builder.Build();
await host.RunAsync().ConfigureAwait(false);
