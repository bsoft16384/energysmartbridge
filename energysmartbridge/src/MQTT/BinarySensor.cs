using System.Text.Json.Serialization;

internal class BinarySensor : Device {
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public DeviceClass? device_class { get; set; }
}