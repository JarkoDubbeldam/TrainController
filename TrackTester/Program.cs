using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Track;
using Track.Graph;

namespace TrackTester {
  class Program {
    static void Main(string[] args) {
      var json = File.ReadAllText("layout.json");
      var halfParsed = JObject.Parse(json);
      var sections = halfParsed["Sections"].ToDictionary(x => x["Id"].ToObject<int>(), x => x.ToObject<TrackSection>());
      var boundaries = halfParsed["Boundaries"]
        .Select(x => new TrackSectionBoundary {
          Id = x["Id"].ToObject<int>(),
          ConnectedTrackSections = x["ConnectedSections"].Select(y => sections[y.ToObject<int>()]).ToList()
        })
        .ToList();


      var originId = 2;
      var destinationId = 4;

      var distances = boundaries.ToDictionary(x => x.Id, x => x.Id == originId ? 0 : int.MaxValue);
      var shortestParents = new Dictionary<int, (int, TrackSection)> { { originId, (int.MinValue, null) } };
      var queue = boundaries.Select(x => x.Id).ToHashSet();

      var current = originId;
      while (true) {
        var neighbors = boundaries[current - 1]
          .ConnectedTrackSections
          .SelectMany(x => boundaries
            .Where(y => y.ConnectedTrackSections.Contains(x))
            .Select(y => (Boundary: y, Section: x)))
          .Where(x => x.Boundary.Id != current)
          .Where(x => x.Section.SectionId != shortestParents[current].Item2?.SectionId) // To avoid backtracking
          .ToHashSet();

        foreach(var neighbor in neighbors) {
          if (queue.Contains(neighbor.Boundary.Id)) {
            var tentativeDistance = distances[current] + 1;
            if(tentativeDistance < distances[neighbor.Boundary.Id]) {
              distances[neighbor.Boundary.Id] = tentativeDistance;
              shortestParents[neighbor.Boundary.Id] = (current, neighbor.Section);
            }
          }
        }

        queue.Remove(current);
        if(current == destinationId) {
          break;
        }

        current = distances.OrderBy(x => x.Value).Select(x => x.Key).Intersect(queue).First();
        if(distances[current] == int.MaxValue) {
          throw new Exception("Couldn't find path.");
        }
      }

      var stack = new Stack<(int, TrackSection)>();
      stack.Push((current, null));
      while (current != originId) { 
        stack.Push(shortestParents[current]);
        current = shortestParents[current].Item1;
      }

      Console.WriteLine($"From Id {originId} to {destinationId}, visits sections {string.Join(", ", stack.Select(x => x.Item2?.SectionId))}");
      foreach(var turnout in stack.Where(x => x.Item2 != null).SelectMany(x => x.Item2.Turnouts)) {
        Console.WriteLine($"Turnout {turnout.TurnoutId} in setting {turnout.TurnoutMode}");
      }
    }
  }
}
