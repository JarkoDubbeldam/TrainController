using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Newtonsoft.Json.Linq;

using Track;

namespace TrackTester {
  class Program {
    static void Main(string[] args) {
      //var dict = halfParsed["Sections"]
      //  .ToDictionary(x => x["Id"], y => new TrackSection {
      //    Id = y["Id"].ToObject<int>(),
      //    SectionId = y["SectionId"].ToObject<int>(),
      //    Turnouts = y["Turnouts"].ToObject<List<TurnoutConfiguration>>()
      //  });
      //foreach(var section in halfParsed["Sections"]) {
      //  TrackSection trackSection = dict[section["Id"]];
      //  trackSection.ConnectedSections = section["ConnectedSections"]
      //    .Select(x => dict[x])
      //    .Where(x => x.SectionId != trackSection.SectionId)
      //    .ToList();
      //}

      //CreateLayoutV3json();
      var x = ParseLayoutV3json();
      var sw = Stopwatch.StartNew();
      WayFinder.FindRoute(x, 1, 20).ForEach(x => Console.WriteLine(x));
      Console.WriteLine(sw.Elapsed);
    }

    //private static void CreateLayoutV3json() {
    //  var json = File.ReadAllText("layout.json");
    //  var halfParsed = JObject.Parse(json);
    //  var sectionDict = halfParsed["Sections"].ToDictionary(x => x["Id"], y => y.ToObject<TrackSection>());
    //  var boundaries = halfParsed["Boundaries"].Select(y => new TrackSectionBoundary {
    //    Id = y["Id"].ToObject<int>(),
    //    ConnectedTrackSections = y["ConnectedSections"].Select(x => sectionDict[x]).ToList()
    //  })
    //  .ToList();

    //  var sectionBoundaryLookup = boundaries.SelectMany(x => x.ConnectedTrackSections.Select(y => (Section: y, Boundary: x)))
    //    .ToLookup(x => x.Section);

    //  var stuff = boundaries.Select(x => new {
    //    x.Id,
    //    Connections = x.ConnectedTrackSections.Select(y => new {
    //      ViaSection = y,
    //      ToBoundaryId = sectionBoundaryLookup[y].Single(z => z.Boundary != x).Boundary.Id
    //    }).ToList()
    //  }).ToList();
    //  File.WriteAllText("layoutv3.json", JsonConvert.SerializeObject(new { Boundaries = stuff }));
    //}

    private static ICollection<TrackSectionBoundary> ParseLayoutV3json() {
      var json = File.ReadAllText("layoutV3.json");

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

      return boundariesDict.Values;
    }
  }
}
