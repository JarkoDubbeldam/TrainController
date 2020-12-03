using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

using ReactiveUI;

using TrainUI.ViewModels;

namespace TrainUI.Converters {
  internal class TrainViewModelConverter : JsonConverter {
    public override bool CanConvert(Type objectType) => objectType == typeof(TrainOverviewViewModel);
    public override bool CanRead => false;
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
      throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      var typedValue = (TrainOverviewViewModel)value;
      if(typedValue.Content is TrainListViewModel) {
        typedValue.Content = null;
      }
      var originalNullHandling = serializer.NullValueHandling;
      try {
        serializer.Converters.Remove(this);
        serializer.NullValueHandling = NullValueHandling.Ignore;
        serializer.Serialize(writer, value);
      } finally {
        serializer.Converters.Add(this);
        serializer.NullValueHandling = originalNullHandling;
      }
    }

  }
}
