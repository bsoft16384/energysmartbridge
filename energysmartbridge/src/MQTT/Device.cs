using Newtonsoft.Json;

internal class Device
{
    public string name { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string state_topic { get; set; }

    public string availability_topic { get; set; } = $"{Global.mqtt_prefix}/status";

    public string unique_id { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public required DeviceRegistry device { get; init; }
}