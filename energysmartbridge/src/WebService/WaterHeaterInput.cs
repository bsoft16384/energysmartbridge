using Microsoft.AspNetCore.Mvc;

internal readonly record struct WaterHeaterInput(
    [FromForm] string DeviceText, // *MAC address*
    [FromForm] string Password, // *Random string*
    [FromForm] string ModuleApi, // 1.5
    [FromForm] string ModFwVer, // 3.1
    [FromForm] string MasterFwVer, // 06.03
    [FromForm] string MasterModelId, // B1.00
    [FromForm] string DisplayFwVer, // 03.04
    [FromForm] string WifiFwVer, // C2.4.0.3.AO7
    [FromForm] int UpdateRate, // 300
    [FromForm] string Mode, // EnergySmart
    [FromForm] int SetPoint, // 120
    [FromForm] string Units, // F
    [FromForm] string LeakDetect, // NotDetected
    [FromForm] int MaxSetPoint, // 120
    [FromForm] string Grid, // Enabled
    [FromForm] string AirFilterStatus, // OK
    [FromForm] bool CondensePumpFail, // False
    [FromForm] string AvailableModes, // Standard,Vacation,EnergySmart
    [FromForm] bool SystemInHeating, // False
    [FromForm] string HotWaterVol, // High
    [FromForm] string Leak, // None
    [FromForm] string DryFire, // None
    [FromForm] string ElementFail, // None
    [FromForm] string TankSensorFail, // None
    [FromForm] bool EcoError, // False
    [FromForm] string MasterDispFail, // None
    [FromForm] string CompSensorFail, // None
    [FromForm] string SysSensorFail, // None
    [FromForm] string SystemFail, // None
    [FromForm] int UpperTemp, // 122
    [FromForm] int LowerTemp, // 104
    [FromForm] string FaultCodes, // 0
    [FromForm] string UnConnectNumber, // 0
    [FromForm] string AddrData, // *Two strings*
    [FromForm] string SignalStrength // -46
) {
  public string DisplayName => $"Water Heater {DeviceText}";

  public string ToTopic(Topic topic) => $"{Global.mqtt_prefix}/{DeviceText}/{topic}";
  
  public Climate ToThermostatConfig(DeviceRegistry device) => new() {
    device = device,
    name = DisplayName,
    action_template = "{% if value == 'ON' %} heating {%- else -%} off {%- endif %}",
    action_topic = ToTopic(Topic.systeminheating_state),
    current_temperature_topic = ToTopic(Topic.uppertemp_state),
    temperature_state_topic = ToTopic(Topic.setpoint_state),
    temperature_command_topic = ToTopic(Topic.setpoint_command),
    max_temp = MaxSetPoint.ToString(),
    mode_state_topic = ToTopic(Topic.mode_state),
    mode_command_topic = ToTopic(Topic.mode_command),
    modes = [ "eco", "heat_pump", "electric", "off" ],
    unique_id = $"{DeviceText}_water_heater",
  };

  private BinarySensor BinarySensor(DeviceRegistry device, string name, Topic topic, string id) =>
    new() {
      name = $"{DisplayName} {name}",
      state_topic = ToTopic(topic),
      unique_id = $"{DeviceText}_{id}",
      device = device,
    };

  private Sensor Sensor(DeviceRegistry device, string name, Topic topic, string id) =>
    new() {
      name = $"{DisplayName} {name}",
      state_topic = ToTopic(topic),
      unique_id = $"{DeviceText}_{id}",
      device = device,
    };

  public BinarySensor ToInHeatingConfig(DeviceRegistry device) =>
      BinarySensor(device, "Element", Topic.systeminheating_state, "is_heating");

  public Sensor ToRawModeConfig(DeviceRegistry device) =>
      Sensor(device, "Raw Mode", Topic.raw_mode_state, "raw_mode");

  public BinarySensor ToGridConfig(DeviceRegistry device) =>
      BinarySensor(device, "RA Enabled", Topic.grid_state, "ra_enabled");

  public BinarySensor ToAirFilterStatusConfig(DeviceRegistry device) => 
      BinarySensor(device, "Air Filter Status", Topic.air_filter_status_state, "air_filter_status");

  public BinarySensor ToCondensePumpFailConfig(DeviceRegistry device) => BinarySensor(
      device, "Condensate Pump Fail", Topic.condense_pump_fail_state, "condensate_pump_fail");

  public BinarySensor ToLeakDetectConfig(DeviceRegistry device) => 
      BinarySensor(device, "Leak Detect", Topic.leak_detect_state, "leak_detect");

  public Sensor ToHotWaterVolConfig(DeviceRegistry device) =>
      Sensor(device, "Hot Water Volume", Topic.hotwatervol_state, "hot_water_volume");
  
  public Sensor ToUpperTempConfig(DeviceRegistry device) => new() {
    device = device,
    name = DisplayName + " Upper",
    device_class = DeviceClass.temperature,
    state_topic = ToTopic(Topic.uppertemp_state),
    unit_of_measurement = "°" + Units,
    unique_id = DeviceText + "_upper_temp",
  };

  public Sensor ToLowerTempConfig(DeviceRegistry device) => new() {
    device = device,
    name = DisplayName + " Lower",
    device_class = DeviceClass.temperature,
    state_topic = ToTopic(Topic.lowertemp_state),
    unit_of_measurement = "°" + Units,
    unique_id = DeviceText + "_lower_temp",
  };

  public BinarySensor ToDryFireConfig(DeviceRegistry device) => 
      BinarySensor(device, "Dry Fire", Topic.dryfire_state, "dry_fire");

  public BinarySensor ToElementFailConfig(DeviceRegistry device) =>
      BinarySensor(device, "Element Fail", Topic.elementfail_state, "element_fail");

  public BinarySensor ToTankSensorFailConfig(DeviceRegistry device) =>
      BinarySensor(device, "Tank Sensor Fail", Topic.tanksensorfail_state, "tank_sensor_fail");

  public BinarySensor ToEcoErrorConfig(DeviceRegistry device) =>
      BinarySensor(device, "Eco Error", Topic.eco_error_state, "eco_error");

  public Sensor ToLeakConfig(DeviceRegistry device) => 
      Sensor(device, "Leak", Topic.leak_state, "leak");

  public Sensor ToMasterDispFailConfig(DeviceRegistry device) => Sensor(
      device, "Master Display Fail", Topic.master_disp_fail_state, "master_display_fail");

  public Sensor ToCompSensorFailConfig(DeviceRegistry device) => Sensor(
      device, "Compressor Sensor Fail", Topic.comp_sensor_fail_state, "compressor_sensor_fail");

  public Sensor ToSysSensorFailConfig(DeviceRegistry device) =>
      Sensor(device, "System Sensor Fail", Topic.sys_sensor_fail_state, "system_sensor_fail");

  public Sensor ToSystemFailConfig(DeviceRegistry device) =>
      Sensor(device, "System Fail", Topic.system_fail_state, "system_fial");

  public Sensor ToFaultCodesConfig(DeviceRegistry device) => 
      Sensor(device, "System Fail", Topic.fault_codes_state, "fault_codes");  
}