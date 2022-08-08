using System.Collections.Generic;
using System.Linq;
using Track;
using TrainRepository;

namespace TrainTracker {
  public class TrainLocation {
    private readonly List<TrackConnection> occupiedConnections;

    public Train Train { get; }
    public IReadOnlyList<TrackConnection> OccupiedConnections => occupiedConnections;
    public bool Update(TrackSection changedSection, bool newOccupancy) {
      if (newOccupancy) {
        /*
         * If a section has become occupied:
         * Check if any of the currently occupied sections is next to
         * the new section in the direction the train is heading.
         */
        foreach(var section in occupiedConnections) {
          var newSection = section.WalkActiveSections().First();
          if (newSection.TrackConnectionState == TrackConnection.TrackConnectionIterator.TrackConnectionStateEnum.Active &&
            newSection.TrackConnection.ViaSection == changedSection) {
            occupiedConnections.Add(newSection.TrackConnection);
            return true;
          }
        }
        return false;
      } else {
        /*
         * Else, a section has become unoccupied. If so, check if this
         * train is currently occupying, and if so, free.
         */
        var removedCount = occupiedConnections.RemoveAll(c => c.ViaSection == changedSection);
        return removedCount > 0;
      }
    }

    public override string ToString() {
      var trainname = Train.Name;
      var sections = string.Join(", ", OccupiedConnections.Select(x => x.ViaSection.SectionId));
      return $@"for train {trainname}. 

Sections:
{sections}";
    }
  }
}
