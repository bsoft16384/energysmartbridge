using System.Text.Json.Serialization;

internal class WaterHeaterOutput {
  public string Success { get; init; } = "0";

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? UpdateRate { get; set; }

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? Mode { get; set; }

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? SetPoint { get; set; }
}
