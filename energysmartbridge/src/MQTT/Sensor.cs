using System.Text.Json.Serialization;

internal partial class Sensor : Device {
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
  public DeviceClass? device_class { get; set; }

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? unit_of_measurement { get; set; }
}
