using System;
using System.Collections.Generic;
using System.Text;

namespace Z21.API {
  public class TurnoutChangingEventArgs {
    internal TurnoutChangingEventArgs(short address) {
      Address = address;
    }

    public short Address { get; }
    public bool Handled { get; set; } = false;
    public TimeSpan? DelayChange { get; set; } = null;
  }
}
