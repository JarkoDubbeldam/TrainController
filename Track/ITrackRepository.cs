using System.Collections.Generic;

namespace Track {
  public interface ITrackRepository {
    IReadOnlyCollection<TrackSectionBoundary> Boundaries { get; }
    IReadOnlyCollection<TrackSection> Sections { get; }
  }
}