using System;
using System.Collections.Generic;
using System.Text;

namespace Track {
  public class TrackConnection {
    public TrackSection ViaSection { get; set; }
    public TrackSectionBoundary ToBoundary { get; set; }
  }
}
