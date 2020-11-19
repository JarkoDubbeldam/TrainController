
using System;

using Newtonsoft.Json;

namespace TrainUI.Converters {
  public abstract class ModelViewModelConverter<TModel, TViewModel> : JsonConverter {

    private readonly Func<TModel, TViewModel> factory;

    public ModelViewModelConverter(Func<TModel, TViewModel> factory) {
      this.factory = factory;
    }

    public sealed override bool CanConvert(Type objectType) => objectType == typeof(TViewModel);// || objectType == typeof(TrainModel);

    public sealed override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
      var model = serializer.Deserialize<TModel>(reader);
      return factory(model);
    }

    public sealed override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      var viewModel = (TViewModel)value;
      serializer.Serialize(writer, ToModel(viewModel));
    }

    public abstract TModel ToModel(TViewModel viewModel);

  }

}
