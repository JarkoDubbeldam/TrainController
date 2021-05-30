using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json.Linq;

namespace Track {
  public class TrackRepository : ITrackRepository {
    private readonly List<TrackSection> trackSections;
    private readonly List<TrackSectionBoundary> boundaries;

    private TrackRepository(List<TrackSection> trackSections, List<TrackSectionBoundary> boundaries) {
      this.trackSections = trackSections;
      this.boundaries = boundaries;
    }

    public IReadOnlyCollection<TrackSection> Sections => trackSections;
    public IReadOnlyCollection<TrackSectionBoundary> Boundaries => boundaries;

    public static TrackRepository FromJson(string json) {
      var halfParsed = JObject.Parse(json);
      var boundariesDict = halfParsed["Boundaries"].ToDictionary(x => x["Id"], y => new TrackSectionBoundary {
        Id = y["Id"].ToObject<int>()
      });

      foreach (var item in halfParsed["Boundaries"]) {
        var boundary = boundariesDict[item["Id"]];
        boundary.Connections = item["Connections"]
          .Select(x => new TrackConnection {
            ViaSection = x["ViaSection"].ToObject<TrackSection>(),
            ToBoundary = boundariesDict[x["ToBoundaryId"]]
          })
          .ToList();
      }
      var sections = boundariesDict.Values
        .SelectMany(x => x.Connections.Select(y => y.ViaSection))
        .GroupBy(x => x.Id, (key, element) => element.First())
        .ToDictionary(x => x.Id);
      foreach (var boundary in boundariesDict.Values) {
        foreach (var connection in boundary.Connections) {
          connection.ViaSection = sections[connection.ViaSection.Id];
          connection.ViaSection.ConnectedBoundaries.Add(boundary);
        }
      }

      return new TrackRepository(sections.Values.ToList(), boundariesDict.Values.ToList());
    }
  }
}
