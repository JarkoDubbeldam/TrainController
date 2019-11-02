using System;
using System.Collections.Generic;
using System.Text;

namespace Z21.Domain {
  public enum TrackStatus : byte {
    Off = 0x00,
    On = 0x01,
    ProgrammingMode = 0x02,
    ShortCircuit = 0x08
  }
}
