using System;
using System.Linq;
using System.Net.Security;
using System.Runtime.Versioning;
using System.Security.Authentication;
using EnergySmartBridge.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

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
      });
    });
    Settings.LoadSettings(builder.Configuration);
    var mqttModule = new MQTTModule();
    var app = builder.Build();
    app.MapPost("/~branecky/postAll.php", mqttModule.ProcessRequest);
    app.Run();
  }
}
