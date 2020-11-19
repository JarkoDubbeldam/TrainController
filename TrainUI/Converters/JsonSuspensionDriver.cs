using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

using Newtonsoft.Json;

using ReactiveUI;

using TrainUI.ViewModels;

namespace TrainUI.Converters {
  public class JsonSuspensionDriver : ISuspensionDriver {
    private readonly JsonSuspensionSettings jsonSuspensionSettings;

    public JsonSuspensionDriver(JsonSuspensionSettings jsonSuspensionSettings, IList<JsonConverter> jsonConverters) {
      this.jsonSuspensionSettings = jsonSuspensionSettings;
      jsonSuspensionSettings.JsonSerializerSettings.Converters = jsonConverters;
    }
    public IObservable<Unit> InvalidateState() {
      if (File.Exists(jsonSuspensionSettings.Filename)) {
        File.Delete(jsonSuspensionSettings.Filename);
      }
      return Observable.Return(Unit.Default);
    }

    public IObservable<object> LoadState() {
      var json = File.ReadAllText(jsonSuspensionSettings.Filename);
      var state = JsonConvert.DeserializeObject<MainWindowViewModel>(json, jsonSuspensionSettings.JsonSerializerSettings);
      return Observable.Return(state);
    }

    public IObservable<Unit> SaveState(object state) {
      var json = JsonConvert.SerializeObject(state, jsonSuspensionSettings.JsonSerializerSettings);
      File.WriteAllText(jsonSuspensionSettings.Filename, json);
      return Observable.Return(Unit.Default);
    }
  }
}
