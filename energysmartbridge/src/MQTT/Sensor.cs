using Newtonsoft.Json;

internal partial class Sensor : Device {
  [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
  public DeviceClass? device_class { get; set; }

  [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
  public string unit_of_measurement { get; set; }
}
