using System.Text.Json.Serialization;

internal abstract class Device {
    public string name { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? state_topic { get; set; }

    public string availability_topic { get; set; } = $"{Global.mqtt_prefix}/status";

    public string unique_id { get; set; }

    public required DeviceRegistry device { get; init; }
}
