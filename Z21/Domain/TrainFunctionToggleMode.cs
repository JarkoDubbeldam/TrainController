using System;
using System.Collections.Generic;
using System.Text;

namespace Z21.Domain {
  public enum TrainFunctionToggleMode : byte {
    Off = 0,
    On = 0b01000000,
    Toggle = 0b11000000
  }
}
