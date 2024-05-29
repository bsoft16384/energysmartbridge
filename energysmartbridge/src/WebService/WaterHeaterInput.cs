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
  
  public Climate ToThermostatConfig() => new() {
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

  public BinarySensor ToInHeatingConfig() => new() {
    name = DisplayName + " Element",
    state_topic = ToTopic(Topic.systeminheating_state),
    unique_id = DeviceText + "_is_heating",
  };

  public Sensor ToRawModeConfig() => new() {
    name = DisplayName + " Raw Mode",
    state_topic = ToTopic(Topic.raw_mode_state),
    unique_id = DeviceText + "_raw_mode",
  };

  public BinarySensor ToGridConfig() => new() {
    name = DisplayName + " RA Enabled",
    state_topic = ToTopic(Topic.grid_state),
    unique_id = DeviceText + "_ra_enabled",
  };

  public BinarySensor ToAirFilterStatusConfig() => new() {
    name = DisplayName + " Air Filter Status",
    state_topic = ToTopic(Topic.air_filter_status_state),
    unique_id = DeviceText + "_air_filter_status",
  };

  public BinarySensor ToCondensePumpFailConfig() => new() {
    name = DisplayName + " Condense Pump Fail",
    state_topic = ToTopic(Topic.condense_pump_fail_state),
    unique_id = DeviceText + "_condense_pump_fail",
  };

  public BinarySensor ToLeakDetectConfig() => new() {
    name = DisplayName + " Leak Detect",
    state_topic = ToTopic(Topic.leak_detect_state),
    unique_id = DeviceText + "_leak_detect",
  };

  public Sensor ToHotWaterVolConfig() => new() {
    name = DisplayName + " Volume",
    state_topic = ToTopic(Topic.hotwatervol_state),
    unique_id = DeviceText + "_volume",
  };

  public Sensor ToUpperTempConfig() => new() {
    name = DisplayName + " Upper",
    device_class = Sensor.DeviceClass.temperature,
    state_topic = ToTopic(Topic.uppertemp_state),
    unit_of_measurement = "°" + Units,
    unique_id = DeviceText + "_upper_temp",
  };

  public Sensor ToLowerTempConfig() => new() {
    name = DisplayName + " Lower",
    device_class = Sensor.DeviceClass.temperature,
    state_topic = ToTopic(Topic.lowertemp_state),
    unit_of_measurement = "°" + Units,
    unique_id = DeviceText + "_lower_temp",
  };

  public BinarySensor ToDryFireConfig() => new() {
    name = DisplayName + " Dry Fire",
    state_topic = ToTopic(Topic.dryfire_state),
    unique_id = DeviceText + "_dry_fire",
  };

  public BinarySensor ToElementFailConfig() => new() {
    name = DisplayName + " Element Fail",
    state_topic = ToTopic(Topic.elementfail_state),
    unique_id = DeviceText + "_element_fail",
  };

  public BinarySensor ToTankSensorFailConfig() => new() {
    name = DisplayName + " Tank Sensor Fail",
    state_topic = ToTopic(Topic.tanksensorfail_state),
    unique_id = DeviceText + "_tank_sensor_fail",
  };

  public BinarySensor ToEcoErrorConfig() => new() {
    name = DisplayName + " Eco Error",
    state_topic = ToTopic(Topic.tanksensorfail_state),
    unique_id = DeviceText + "_eco_error",
  };

  public Sensor ToLeakConfig() => new() {
    name = DisplayName + " Leak",
    state_topic = ToTopic(Topic.leak_state),
    unique_id = DeviceText + "_leak",
  };

  public Sensor ToMasterDispFailConfig() => new() {
    name = DisplayName + " Master Disp Fail",
    state_topic = ToTopic(Topic.master_disp_fail_state),
    unique_id = DeviceText + "_master_disp_fail",
  };

  public Sensor ToCompSensorFailConfig() => new() {
    name = DisplayName + " Comp Sensor Fail",
    state_topic = ToTopic(Topic.comp_sensor_fail_state),
    unique_id = DeviceText + "_comp_sensor_fail",
  };

  public Sensor ToSysSensorFailConfig() => new() {
    name = DisplayName + " Sys Sensor Fail",
    state_topic = ToTopic(Topic.sys_sensor_fail_state),
    unique_id = DeviceText + "_sys_sensor_fail",
  };

  public Sensor ToSystemFailConfig() => new() {
    name = DisplayName + " System Fail",
    state_topic = ToTopic(Topic.system_fail_state),
    unique_id = DeviceText + "_system_fail",
  };

  public Sensor ToFaultCodesConfig() => new() {
    name = DisplayName + " Fault Codes",
    state_topic = ToTopic(Topic.fault_codes_state),
    unique_id = DeviceText + "_fault_codes",
  };    
}