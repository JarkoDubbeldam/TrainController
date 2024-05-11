using System;
using System.Collections.Generic;
using System.Linq;

using Z21.Domain;

namespace Z21.API {
  public class SetSignalRequest : Request {
    public short Address { get; set; }
    public required SignalMode SignalMode { get; set; }

    private static readonly Dictionary<(SignalColour, bool, bool), TurnoutPosition[]> mappings =
      new() {
        [(SignalColour.Red, false, false)] = [TurnoutPosition.Position1, TurnoutPosition.Position1, TurnoutPosition.Position1],
        [(SignalColour.Yellow, false, false)] = [TurnoutPosition.Position1, TurnoutPosition.Position2, TurnoutPosition.Position1],
        [(SignalColour.Yellow, false, true)] = [TurnoutPosition.Position1, TurnoutPosition.Position1, TurnoutPosition.Position2],
        [(SignalColour.Yellow, true, false)] = [TurnoutPosition.Position1, TurnoutPosition.Position2, TurnoutPosition.Position2],
        [(SignalColour.Yellow, true, true)] = [TurnoutPosition.Position2, TurnoutPosition.Position2, TurnoutPosition.Position2],
        [(SignalColour.Green, false, false)] = [TurnoutPosition.Position2, TurnoutPosition.Position1, TurnoutPosition.Position1],
        [(SignalColour.Green, true, false)] = [TurnoutPosition.Position2, TurnoutPosition.Position1, TurnoutPosition.Position2],
        [(SignalColour.Green, true, true)] = [TurnoutPosition.Position2, TurnoutPosition.Position2, TurnoutPosition.Position1],
      };
    private static readonly int[] commandOrder = [1, 0, 2]; // TODO figure out better transition logic so that yellow -> green doesn't go past red.

    internal override byte[] ToByteArray() {
      if (!mappings.TryGetValue((SignalMode.SignalColour, SignalMode.Blinking, SignalMode.Number), out var mapping)) {
        throw new InvalidOperationException("Unavailable combinations of settings");
      }

      return commandOrder.Select(idx => new SetTurnoutRequest {
        Address = (short)(Address + idx),
        TurnoutPosition = mapping[idx]
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
