using EnergySmartBridge.WebService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


internal class MqttService : IHostedService {
  private static readonly FrozenSet<Topic> _topicsToSubscribe = new HashSet<Topic>() {
    Topic.updaterate_command,
    Topic.mode_command,
    Topic.setpoint_command
  }.ToFrozenSet();

  private DeviceRegistry _deviceRegistry;

  private IManagedMqttClient _mqttClient = new MqttFactory().CreateManagedMqttClient();

  private readonly Regex regexTopic = new(Global.mqtt_prefix + "/([A-F0-9]+)/(.*)", RegexOptions.Compiled);

  private readonly ConcurrentDictionary<string, Queue<WaterHeaterOutput>> connectedModules = new();

  private readonly ILogger<MqttService> _logger;

  public MqttService(ILogger<MqttService> logger) {
    _logger = logger;
    
    _deviceRegistry = new DeviceRegistry() {
        identifiers = Global.mqtt_prefix,
        name = Global.mqtt_prefix,
        sw_version = $"EnergySmartBridge {Assembly.GetExecutingAssembly().GetName().Version}",
        model = "Water Heater Controller",
        manufacturer = "EnergySmart"
    };

    _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate((e) =>
    {
        _logger.LogDebug("Connected");

        // Clear cache so we publish config on next check-in
        connectedModules.Clear();

        _logger.LogDebug("Publishing controller online");
        PublishAsync($"{Global.mqtt_prefix}/status", "online");
    });
    _mqttClient.ApplicationMessageReceivedHandler =
        new MqttApplicationMessageReceivedHandlerDelegate(OnAppMessage);
    _mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(
        e => _logger.LogDebug("Error connecting " + e.Exception.Message));
    _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(
        e => _logger.LogDebug("Disconnected"));          
  }

  public async Task StartAsync(CancellationToken cancellationToken) {
    var willMessage = new MqttApplicationMessage() {
      Topic = $"{Global.mqtt_prefix}/status",
      Payload = Encoding.UTF8.GetBytes("offline"),
      QualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce,
      Retain = true
    };

    MqttClientOptionsBuilder options = new MqttClientOptionsBuilder()
        .WithTcpServer(Global.mqtt_server)
        .WithWillMessage(willMessage);

    if (!string.IsNullOrEmpty(Global.mqtt_username))
        options = options
            .WithCredentials(Global.mqtt_username, Global.mqtt_password);

    ManagedMqttClientOptions manoptions = new ManagedMqttClientOptionsBuilder()
      .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
      .WithClientOptions(options.Build())
      .Build();

    await _mqttClient.StartAsync(manoptions);


    foreach (var topic in _topicsToSubscribe) {
      var topicFilter = new MqttTopicFilterBuilder()
          .WithTopic($"{Global.mqtt_prefix}/+/{topic}")
          .Build();
      await _mqttClient.SubscribeAsync(topicFilter);
    }
  }

      protected virtual void OnAppMessage(MqttApplicationMessageReceivedEventArgs e)
      {
          Match match = regexTopic.Match(e.ApplicationMessage.Topic);

          if (!match.Success)
              return;

          if (!Enum.TryParse(match.Groups[2].Value, true, out Topic topic))
              return;

          string id = match.Groups[1].Value;
          string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

          _logger.LogDebug($"Received: Id: {id}, Command: {topic}, Value: {payload}");

          if(connectedModules.TryGetValue(id, out var updateQueue))
          {
              if (topic == Topic.updaterate_command && 
                  int.TryParse(payload, out int updateRate) && updateRate >= 30 && updateRate <= 300)
              {
                  _logger.LogDebug($"Queued {id} UpdateRate: {updateRate}");
              updateQueue.Enqueue(new WaterHeaterOutput()
                  {
                      UpdateRate = updateRate.ToString()
                  });
              }
              else if (topic == Topic.mode_command)
              {
                  _logger.LogDebug($"Queued {id} Mode: {payload}");

                  string heater_mode = "off";
                  switch(payload) {
                      case "heat_pump":
                          heater_mode = "Efficiency";
                          break;
                      case "eco":
                          heater_mode = "Hybrid";
                          break;
                      case "electric":
                          heater_mode = "Electric";
                          break;
                      default:
                          break;
                  }

              updateQueue.Enqueue(new WaterHeaterOutput()
                  {
                      Mode = heater_mode
                  });
              }
              else if (topic == Topic.setpoint_command &&
                  double.TryParse(payload, out double setPoint) && setPoint >= 80 && setPoint <= 150)
              {
                  _logger.LogDebug($"Queued {id} SetPoint: {((int)setPoint)}");
              updateQueue.Enqueue(new WaterHeaterOutput()
                  {
                      SetPoint = ((int)setPoint).ToString()
                  });
              }
          }
      }

      public async Task StopAsync(CancellationToken cancellationToken)
      {
          _logger.LogDebug("Publishing controller offline");
          await PublishAsync($"{Global.mqtt_prefix}/status", "offline");
          await _mqttClient.StopAsync();
      }

      public object ProcessRequest([AsParameters] WaterHeaterInput waterHeater)
      {
          Console.WriteLine(waterHeater);
  
          if(!connectedModules.ContainsKey(waterHeater.DeviceText))
          {
              _logger.LogDebug($"Publishing water heater config {waterHeater.DeviceText}");
              PublishWaterHeater(waterHeater);
              connectedModules.TryAdd(waterHeater.DeviceText, new Queue<WaterHeaterOutput>());
          }

          _logger.LogDebug($"Publishing water heater state {waterHeater.DeviceText}");
          PublishWaterHeaterState(waterHeater);

          if (connectedModules[waterHeater.DeviceText].Count > 0)
          {
              _logger.LogDebug($"Sent queued command {waterHeater.DeviceText}");
              return connectedModules[waterHeater.DeviceText].Dequeue();
          }
          else
          {
              return new WaterHeaterOutput() { };
          }
      }

      private void PublishWaterHeater(WaterHeaterInput waterHeater)
      {
          PublishAsync($"{Global.mqtt_discovery_prefix}/water_heater/{waterHeater.DeviceText}/config",
              JsonConvert.SerializeObject(waterHeater.ToThermostatConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/binary_sensor/{waterHeater.DeviceText}/heating/config",
              JsonConvert.SerializeObject(waterHeater.ToInHeatingConfig(_deviceRegistry)));
          
          PublishAsync($"{Global.mqtt_discovery_prefix}/binary_sensor/{waterHeater.DeviceText}/grid/config",
              JsonConvert.SerializeObject(waterHeater.ToGridConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/binary_sensor/{waterHeater.DeviceText}/airfilterstatus/config",
              JsonConvert.SerializeObject(waterHeater.ToAirFilterStatusConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/binary_sensor/{waterHeater.DeviceText}/condensepumpfail/config",
              JsonConvert.SerializeObject(waterHeater.ToCondensePumpFailConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/binary_sensor/{waterHeater.DeviceText}/leakdetect/config",
              JsonConvert.SerializeObject(waterHeater.ToLeakDetectConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/binary_sensor/{waterHeater.DeviceText}/ecoerror/config",
              JsonConvert.SerializeObject(waterHeater.ToEcoErrorConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/rawmode/config",
              JsonConvert.SerializeObject(waterHeater.ToRawModeConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/hotwatervol/config",
              JsonConvert.SerializeObject(waterHeater.ToHotWaterVolConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/uppertemp/config",
              JsonConvert.SerializeObject(waterHeater.ToUpperTempConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/lowertemp/config",
              JsonConvert.SerializeObject(waterHeater.ToLowerTempConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/dryfire/config",
              JsonConvert.SerializeObject(waterHeater.ToDryFireConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/elementfail/config",
              JsonConvert.SerializeObject(waterHeater.ToElementFailConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/tanksensorfail/config",
              JsonConvert.SerializeObject(waterHeater.ToTankSensorFailConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/leak/config",
              JsonConvert.SerializeObject(waterHeater.ToLeakConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/masterdispfail/config",
              JsonConvert.SerializeObject(waterHeater.ToMasterDispFailConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/compsensorfail/config",
              JsonConvert.SerializeObject(waterHeater.ToCompSensorFailConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/syssensorfail/config",
              JsonConvert.SerializeObject(waterHeater.ToSysSensorFailConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/systemfail/config",
              JsonConvert.SerializeObject(waterHeater.ToSystemFailConfig(_deviceRegistry)));

          PublishAsync($"{Global.mqtt_discovery_prefix}/sensor/{waterHeater.DeviceText}/faultcodes/config",
              JsonConvert.SerializeObject(waterHeater.ToFaultCodesConfig(_deviceRegistry)));
      }

      private void PublishWaterHeaterState(WaterHeaterInput waterHeater)
      {
          PublishAsync(waterHeater.ToTopic(Topic.maxsetpoint_state), waterHeater.MaxSetPoint.ToString());
          PublishAsync(waterHeater.ToTopic(Topic.setpoint_state), waterHeater.SetPoint.ToString());

          string ha_mode = "off";
          switch(waterHeater.Mode) {
              case "Efficiency":
                  ha_mode = "heat_pump";
                  break;
              case "Hybrid":
                  ha_mode = "eco";
                  break;
              case "Electric":
                  ha_mode = "electric";
                  break;
              default:
                  break;
          }

          PublishAsync(waterHeater.ToTopic(Topic.mode_state), ha_mode);

          PublishAsync(waterHeater.ToTopic(Topic.systeminheating_state), waterHeater.SystemInHeating ? "ON" : "OFF");
          PublishAsync(waterHeater.ToTopic(Topic.hotwatervol_state), waterHeater.HotWaterVol);

          PublishAsync(waterHeater.ToTopic(Topic.uppertemp_state), waterHeater.UpperTemp.ToString());
          PublishAsync(waterHeater.ToTopic(Topic.lowertemp_state), waterHeater.LowerTemp.ToString());

          PublishAsync(waterHeater.ToTopic(Topic.updaterate_state), waterHeater.UpdateRate.ToString());

          PublishAsync(waterHeater.ToTopic(Topic.dryfire_state), waterHeater.DryFire);
          PublishAsync(waterHeater.ToTopic(Topic.elementfail_state), waterHeater.ElementFail);
          PublishAsync(waterHeater.ToTopic(Topic.tanksensorfail_state), waterHeater.TankSensorFail);

          PublishAsync(waterHeater.ToTopic(Topic.faultcodes_state), waterHeater.FaultCodes);

          PublishAsync(waterHeater.ToTopic(Topic.signalstrength_state), waterHeater.SignalStrength);

          PublishAsync(waterHeater.ToTopic(Topic.raw_mode_state), waterHeater.Mode);

          PublishAsync(waterHeater.ToTopic(Topic.grid_state), waterHeater.Grid == "Disabled" ? "OFF" : "ON");
          PublishAsync(waterHeater.ToTopic(Topic.air_filter_status_state), waterHeater.AirFilterStatus == "OK" ? "ON" : "OFF");
          PublishAsync(waterHeater.ToTopic(Topic.condense_pump_fail_state), waterHeater.CondensePumpFail ? "ON" : "OFF");
          PublishAsync(waterHeater.ToTopic(Topic.leak_detect_state), waterHeater.LeakDetect == "NotDetected" ? "OFF" : "ON");
          PublishAsync(waterHeater.ToTopic(Topic.eco_error_state), waterHeater.EcoError ? "ON" : "OFF" );

          PublishAsync(waterHeater.ToTopic(Topic.leak_state), waterHeater.Leak.ToString());
          PublishAsync(waterHeater.ToTopic(Topic.master_disp_fail_state), waterHeater.MasterDispFail.ToString());
          PublishAsync(waterHeater.ToTopic(Topic.comp_sensor_fail_state), waterHeater.CompSensorFail.ToString());
          PublishAsync(waterHeater.ToTopic(Topic.sys_sensor_fail_state), waterHeater.SysSensorFail.ToString());
          PublishAsync(waterHeater.ToTopic(Topic.system_fail_state), waterHeater.SystemFail.ToString());
          PublishAsync(waterHeater.ToTopic(Topic.fault_codes_state), waterHeater.FaultCodes.ToString());
      }

      private Task PublishAsync(string topic, string payload)
      {
          return _mqttClient.PublishAsync(topic, payload, MqttQualityOfServiceLevel.AtMostOnce, true);
      }
}