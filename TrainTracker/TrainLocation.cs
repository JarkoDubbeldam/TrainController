using System.Collections.Generic;
using System.Linq;
using Track;
using TrainRepository;

namespace TrainTracker {
  public class TrainLocation {
    private List<TrackConnection> occupiedConnections;

    public TrainLocation(Train train, IEnumerable<TrackConnection> trackConnections) {
      occupiedConnections = trackConnections.ToList();
      Train = train;
    }

    public Train Train { get; }
    public IReadOnlyList<TrackConnection> OccupiedConnections => occupiedConnections;
    public bool TrainLost { get; private set; }
    public bool Update(TrackSection changedSection, bool newOccupancy) {
      if (newOccupancy) {
        /*
         * If a section has become occupied:
         * Check if any of the currently occupied sections is next to
         * the new section in the direction the train is heading.
         */
        foreach (var section in occupiedConnections) {
          if (section.ViaSection == changedSection) {
            TrainLost = false;
            return true;
          }
          var newSection = section.WalkActiveSections().First();
          if (newSection.TrackConnectionState == TrackConnection.TrackConnectionIterator.TrackConnectionStateEnum.Active &&
            newSection.TrackConnection.ViaSection == changedSection) {
            occupiedConnections.Add(newSection.TrackConnection);
            TrainLost = false;
            return true;
          }

        }
        return false;
      } else {
        /*
         * Else, a section has become unoccupied. If so, check if this
         * train is currently occupying, and if so, free.
         */
        var toRemove = new List<TrackConnection>();
        foreach (var occupiedSection in occupiedConnections) {
          if (occupiedSection.ViaSection == changedSection) {
            toRemove.Add(occupiedSection);
          }
        }
        /*
         * If removing would empty the list, set the train to lost instead.
         */
        if (toRemove.Count == 0) {
          return false;
        } else if (occupiedConnections.Count - toRemove.Count == 0) {
          TrainLost = true;
          return true;
        } else {
          occupiedConnections.RemoveAll(c => toRemove.Contains(c));
          return true;
        }
      }
    }

    public void TurnAround() {
      var newConnections = occupiedConnections.Select(occupiedSection => occupiedSection.ToBoundary.Connections
          .Where(x => x.ViaSection.SectionId == occupiedSection.ViaSection.SectionId)
          .Where(x => x.ViaSection.IsActive)
          .Single())
        .ToList();
      occupiedConnections = newConnections;
    }

    public override string ToString() {
      var trainname = Train.Name;
      var sections = string.Join(", ", OccupiedConnections.Select(x => x.ViaSection.SectionId));
      var snippet = TrainLost ? "Last seen" : "Sections";
      return $@"for train {trainname}. 

{snippet}: {sections}";
    }
  }
}
