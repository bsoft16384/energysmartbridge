using System.Runtime.Versioning;
using System.Security.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

[assembly:SupportedOSPlatform("linux")]

namespace EnergySmartBridge;

internal class Program {
  internal static void Main(string[] args) {
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.ConfigureKestrel(kestrel => 
      kestrel.ListenAnyIP(443, listenOptions => listenOptions.UseHttps(h => {
        h.ServerCertificate = Certificate.SelfSignedCertificate;
        h.SslProtocols = SslProtocols.Tls;
      }))
    );
    builder.Services.AddHostedService<MqttService>();
    Settings.LoadSettings(builder.Configuration);
    var app = builder.Build();
    app.MapPost("/~branecky/postAll.php", ProcessRequest).DisableAntiforgery();
    app.Run();
  }

  internal static object ProcessRequest(
      [AsParameters] WaterHeaterInput waterHeater, MqttService mqttService) =>
    mqttService.ProcessRequest(waterHeater);
}
