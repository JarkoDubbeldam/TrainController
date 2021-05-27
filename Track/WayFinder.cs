using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Track {
  public static class WayFinder {
    public static List<RouteInConstruction> FindRoute(ICollection<TrackSectionBoundary> boundaries, int from, int to) {
      var boundariesDict = boundaries.ToDictionary(x => x.Id);
      return boundariesDict[from].Connections.Select(x => RouteInConstruction.Create(x))
        .Select(FindShortestRoute)
        .Where(x => x != null)
        .ToList();

      RouteInConstruction FindShortestRoute(RouteInConstruction from) {
        Debug.WriteLine(from);
        var boundary = from.Head.ToBoundary;
        if(boundary.Id == to) {
          return from;
        }
        var previousSection = from.Head.ViaSection;
        var options = boundary.Connections
          // Avoid backtracking
          .Where(x => x.ViaSection.SectionId != previousSection.SectionId)
          // Only add section if not already included in the route so far (i.e., stop searching if we're looping back on ourselves)
          .WhereSelect(x => { 
            var predicate = from.TryAppend(x, out var result);
            return (predicate, result);
          })
          .Select(x => FindShortestRoute(x))
          // Filter options that didn't result in a route.
          .Where(x => x != null);
        // Only return the shortest option. If there are none, default (= null) is returned.
        return options.MinBy(x => x.Depth);
      }
    }
  }
}
