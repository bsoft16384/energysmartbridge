using System.Runtime.Versioning;
using System.Security.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

[assembly:SupportedOSPlatform("linux")]

namespace EnergySmartBridge;

class Program {
  static void Main(string[] args) {
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.ConfigureKestrel(kestrel => {
      kestrel.ListenAnyIP(443, listenOptions => {
        listenOptions.UseHttps(h => {
          h.ServerCertificate = Certificate.SelfSignedCertificate;
          h.SslProtocols = SslProtocols.Tls;
        });
        listenOptions.Protocols = HttpProtocols.Http1;
      });
    });
    Settings.LoadSettings(builder.Configuration);
    var mqttModule = new MQTTModule();
    var app = builder.Build();
    app.MapPost("/~branecky/postAll.php", mqttModule.ProcessRequest)
        .DisableAntiforgery();
    app.Run();
  }
}
