using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

internal class Sensor : Device {
  [JsonConverter(typeof(StringEnumConverter))]
  public enum DeviceClass {
    temperature
  }

  [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
  public DeviceClass? device_class { get; set; }

  [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
  public string unit_of_measurement { get; set; }
}
