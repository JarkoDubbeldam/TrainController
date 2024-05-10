using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainController.Signals;
public class Signal {
  public int Id { get; set; }
  public SignalMode? SignalMode { get; set; }
  public SignalMode? SignalStatus { get; set; }
  public long Timestamp { get; set; }
}

public record SignalMode(SignalColour SignalColour, bool Blinking, bool ShowNumber, bool NightMode);

public enum SignalColour {
  Green,
  Yellow,
  Red
}
