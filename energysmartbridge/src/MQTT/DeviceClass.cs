using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
internal enum DeviceClass {
  temperature
}
