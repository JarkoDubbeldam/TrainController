using System;
using System.Collections.Generic;
using System.Text;

namespace Z21.Domain {
  public class SignalMode {
    public SignalColour SignalColour { get; set; }
    public bool Blinking { get; set; }
    public bool Number { get; set; }
    public bool NightMode { get; set; }
  }

  public enum SignalColour {
    Green,
    Yellow,
    Red
  }
}
