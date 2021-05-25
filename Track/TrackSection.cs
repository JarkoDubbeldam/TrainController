using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Track {
  [DebuggerDisplay("({SectionId})")]
  public class TrackSection {
    public int Id { get; set; }
    public int SectionId { get; set; }
    public List<TurnoutConfiguration> Turnouts { get; set; }
  }
}
