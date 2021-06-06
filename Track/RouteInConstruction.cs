using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Track {
  public class RouteInConstruction {
    private readonly ImmutableList<TrackConnection> route;
    private readonly ImmutableHashSet<int> visitedSections;
    private RouteInConstruction() {
      route = ImmutableList<TrackConnection>.Empty;
      visitedSections = ImmutableHashSet<int>.Empty;
    }
    

    private RouteInConstruction(ImmutableList<TrackConnection> route, ImmutableHashSet<int> visitedSections) {
      this.route = route;
      this.visitedSections = visitedSections;
    }

    public int Depth => route.Count;
    public TrackConnection Head => route[^1];

    public bool TryAppend(TrackConnection trackSection, out RouteInConstruction result) {
      if (visitedSections.Contains(trackSection.ViaSection.Id)) {
        result = null;
        return false;
      }

      var newRoute = route.Add(trackSection);
      var newSet = visitedSections.Add(trackSection.ViaSection.Id);
      result = new RouteInConstruction(newRoute, newSet);
      return true;
    }

    public static RouteInConstruction Empty => new RouteInConstruction();

    public static RouteInConstruction Create(TrackConnection section) {
      Empty.TryAppend(section, out var result);
      return result;
    }

    public override string ToString() {
      return string.Join(" -> ", route.Select(x => (x.ViaSection.Id,x.ViaSection.SectionId).ToString()));
    }
  }
}
