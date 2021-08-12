using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Track;

using TrainUI.Models;
using TrainUI.ViewModels;

namespace TrainUI.Converters {
  public class TrackViewModelConverter : JsonConverter {
    public override bool CanConvert(Type objectType) => objectType == typeof(TrackViewModel);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
      if (objectType != typeof(TrackViewModel)) {
        throw new NotImplementedException();
      }

      var typedValue = (TrackViewModel)existingValue;

      var jobject = JObject.Load(reader);

      var json = jobject["LayoutJson"].ToObject<string>();
      using (var _ = typedValue.SuppressChangeNotifications()) {
        typedValue.TrackPiecesJson = json;
      }
      var repos = TrackRepository.FromJson(json);
      var boundaryLocations = jobject["BoundaryLocations"].ToObject<Dictionary<int, TrackSectionBoundaryModel>>();
      var sectionLocations = jobject["SectionLocations"]
        .Skip(1)
        .Cast<JProperty>()
        .ToDictionary(x => int.Parse(x.Name), x => new IntermediaryType2 {
          ControlPoint1 = new Point(x.Value["ControlPoint1"]["X"].ToObject<int>(), x.Value["ControlPoint1"]["Y"].ToObject<int>()),
          ControlPoint2 = new Point(x.Value["ControlPoint1"]["X"].ToObject<int>(), x.Value["ControlPoint2"]["Y"].ToObject<int>())
        });

      typedValue.TrackSections = new ObservableCollection<TrackSectionViewModel>(repos.Sections.Select(x => {
        if (!sectionLocations.TryGetValue(x.Id, out var section)) {
          section = new IntermediaryType2 { ControlPoint1 = GetDefaultPoint(), ControlPoint2 = GetDefaultPoint() };
        }

        return new TrackSectionViewModel {
          TrackSectionModel = new TrackSectionModel {
            TrackSection = x,
            ControlPoint1 = section.ControlPoint1,
            ControlPoint2 = section.ControlPoint2,
            Boundary1 = GetBoundaryLocation(0, x, boundaryLocations),
            Boundary2 = GetBoundaryLocation(1, x, boundaryLocations)
          }
        };
      }));

      typedValue.TrackRepository = repos;

      return typedValue;
    }

    private static TrackSectionBoundaryModel GetBoundaryLocation(int index, TrackSection x, Dictionary<int, TrackSectionBoundaryModel> boundaryLocations) {
      var id = x.ConnectedBoundaries[index].Id;
      if (!boundaryLocations.TryGetValue(id, out var location)){
        return new TrackSectionBoundaryModel { Id = id, Location = GetDefaultPoint() };      
      }
      return location;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      var typedValue = (TrackViewModel)value;
      var boundaries = typedValue.TrackSections.SelectMany(x => new[] { x.TrackSectionModel.Boundary1, x.TrackSectionModel.Boundary2 })
        .Distinct()
        .ToDictionary(x => x.Id);
      var sections = typedValue.TrackSections.ToDictionary(x => x.TrackSectionModel.TrackSection.Id, x => new IntermediaryType2 {
        ControlPoint1 = x.TrackSectionModel.ControlPoint1,
        ControlPoint2 = x.TrackSectionModel.ControlPoint2
      });

      serializer.Serialize(writer, new IntermediaryType {
        BoundaryLocations = boundaries,
        SectionLocations = sections,
        LayoutJson = typedValue.TrackPiecesJson
      });
    }

    private class IntermediaryType {
      public Dictionary<int, TrackSectionBoundaryModel> BoundaryLocations { get; internal set; }
      public string LayoutJson { get; internal set; }
      public Dictionary<int, IntermediaryType2> SectionLocations { get; internal set; }
    }

    private class IntermediaryType2 {
      public Point ControlPoint1 { get; internal set; }
      public Point ControlPoint2 { get; internal set; }
    }
    private static readonly Random random = new Random();
    private static Point GetDefaultPoint() => new Point(random.Next(100, 200), random.Next(100, 200));
  }
}
