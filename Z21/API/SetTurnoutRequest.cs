using System;
using System.Collections.Generic;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  public class SetTurnoutRequest : AddressSpecificRequest<TurnoutInformation> {
    public short Address { get; set; }
    public Activation Activation { get; set; } = Activation.Activate;
    public TurnoutPosition TurnoutPosition { get; set; }
    public bool QueueMode { get; set; } = true;

    internal override ResponseFactory<TurnoutInformation> GetResponseFactory() {
      return new TurnoutInformationResponseFactory(Address);
    }

    internal override byte[] ToByteArray() {
      if (!QueueMode) {
        throw new NotImplementedException("Queuemode off is currently not supported/implemented");
      }
      if(TurnoutPosition == TurnoutPosition.Unknown) {
        throw new ArgumentException($"Can't set turnout to {TurnoutPosition.Unknown}.");
      }

      Address.GetAddressBytes(out var msb, out var lsb);
      var activationByte = (int)Activation << 3;
      var queueByte = QueueMode ? 0b100000 : 0;
      var positionByte = TurnoutPosition switch
      {
        TurnoutPosition.Position1 => 0,
        TurnoutPosition.Position2 => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(TurnoutPosition))
      };
      var combinedByte = (1 << 7) | activationByte | queueByte | positionByte;

      var bytes = new byte[] { 0x09, 0x00, 0x40, 0x00, 0x53, msb, lsb, (byte)combinedByte, default };
      bytes[8] = (byte)(bytes[4] ^ bytes[5] ^ bytes[6] ^ bytes[7]);
      return bytes;
    }
  }

  public enum Activation {
    Deactivate = 0,
    Activate = 1
  }
}
