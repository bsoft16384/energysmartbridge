using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

internal class BinarySensor : Device {
  [JsonConverter(typeof(StringEnumConverter))]
  public enum DeviceClass {
    heat,
    problem,
  }

  [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
  public DeviceClass? device_class { get; set; }
}