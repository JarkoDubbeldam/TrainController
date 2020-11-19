
using Newtonsoft.Json;

namespace TrainUI.Converters {
  public class JsonSuspensionSettings {
    public string Filename { get; set; }
    public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings { Formatting = Formatting.Indented };
  }
}
