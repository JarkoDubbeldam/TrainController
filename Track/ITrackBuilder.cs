using System;
using System.Collections.Generic;
using System.Text;

namespace Track {
  public interface ITrackBuilder {
    ITrackBuilder AddStraight();
    void LoopToTail();
    void LoopTo(int identifier);
    ICollection<TrackPiece> LooseEnds { get; }
  }
}
