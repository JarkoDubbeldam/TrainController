using System;
using System.Collections.Generic;
using System.Text;

namespace Track {
  public class TrackSectionBoundary {
    public int Id { get; set; }
    public List<TrackSection> ConnectedTrackSections { get; set; }
  }
}
