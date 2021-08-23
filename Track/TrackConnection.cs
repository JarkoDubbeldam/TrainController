using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Track {
  public class TrackConnection {
    public TrackSection ViaSection { get; set; }
    public TrackSectionBoundary ToBoundary { get; set; }
    public SignalConfiguration? Signal { get; set; }

    internal TrackConnection GetNextActiveSection() => 
      ToBoundary.Connections.Where(x => x.ViaSection.SectionId != ViaSection.SectionId).FirstOrDefault(x => x.ViaSection.IsActive);
  }
}
