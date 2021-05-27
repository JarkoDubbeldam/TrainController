using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Track {
  public class TrackSectionBoundary {
    public int Id { get; set; }
    public List<TrackConnection> Connections { get; set; }

  }
}
