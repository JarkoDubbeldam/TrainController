using System;
using System.Collections.Generic;
using System.Linq;

using Z21.Domain;

namespace Z21.API {
  public class SetSignalRequest : Request {
    public short Address { get; set; }
    public SignalMode SignalMode { get; set; }

    private static readonly Dictionary<(SignalColour, bool, bool), TurnoutPosition[]> mappings = 
      new Dictionary<(SignalColour, bool, bool), TurnoutPosition[]>{
        { (SignalColour.Red, false, false), new [] { TurnoutPosition.Position1, TurnoutPosition.Position1, TurnoutPosition.Position1 } },
        { (SignalColour.Green, false, false), new [] { TurnoutPosition.Position2, TurnoutPosition.Position1, TurnoutPosition.Position1 } },
        { (SignalColour.Yellow, false, false), new [] { TurnoutPosition.Position1, TurnoutPosition.Position2, TurnoutPosition.Position1 } },
        { (SignalColour.Green, true, true), new [] { TurnoutPosition.Position2, TurnoutPosition.Position2, TurnoutPosition.Position1 } },
        { (SignalColour.Yellow, false, true), new [] { TurnoutPosition.Position1, TurnoutPosition.Position1, TurnoutPosition.Position2 } },
        { (SignalColour.Green, true, false), new [] { TurnoutPosition.Position2, TurnoutPosition.Position1, TurnoutPosition.Position2 } },
        { (SignalColour.Yellow, true, false), new [] { TurnoutPosition.Position1, TurnoutPosition.Position2, TurnoutPosition.Position2 } },
        { (SignalColour.Yellow, true, true), new [] { TurnoutPosition.Position2, TurnoutPosition.Position2, TurnoutPosition.Position2 } }
    };

    internal override byte[] ToByteArray() {
      if(!mappings.TryGetValue((SignalMode.SignalColour, SignalMode.Blinking, SignalMode.Number), out var mapping)) {
        throw new InvalidOperationException("Unavailable combinations of settings");
      }

      return mapping.Select((position, index) => new SetTurnoutRequest {
        Address = (short)(Address + index),
        TurnoutPosition = position
      })
        .SelectMany(x => x.ToByteArray())
        .Concat(new SetTurnoutRequest {
          Address = (short)(Address + 3),
          TurnoutPosition = SignalMode.NightMode ? TurnoutPosition.Position2 : TurnoutPosition.Position1
        }.ToByteArray())
        .ToArray();
    }
  }
}