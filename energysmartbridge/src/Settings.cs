using System;
using Microsoft.Extensions.Configuration;

namespace EnergySmartBridge
{
    public static class Settings
    {
        public static void LoadSettings(ConfigurationManager settings)
        { 
            Global.mqtt_server = settings["mqtt_server"];
            Global.mqtt_port = settings.ValidatePort();
            Global.mqtt_username = settings["mqtt_username"];
            Global.mqtt_password = settings["mqtt_password"];
            Global.mqtt_prefix = settings["mqtt_prefix"] ?? "energysmart";
            Global.mqtt_discovery_prefix = settings["mqtt_discovery_prefix"] ?? "homeassistant";
        }


        private static int ValidatePort(this ConfigurationManager settings)
        {
            var portString = settings["mqtt_port"];
            if (!int.TryParse(portString, out var port)) {
                throw new Exception($"Invalid mqtt_port: {port}");
            }
            if (port < 1 || port > 65534) {
                throw new Exception($"Invalid mqtt_port number: {port}");
            }
            return port;
        }
    }
}
