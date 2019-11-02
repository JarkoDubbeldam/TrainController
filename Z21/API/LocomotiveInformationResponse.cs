using System;
using System.Collections.Generic;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  internal class LocomotiveInformationResponse : ResponseFactory<LocomotiveInformation> {
    const byte LocomotiveBusyMask = 0b00001000;
    const byte SpeedStepMask = 0b00000111;
    const byte DoubleTractionMask = 0b0100000;
    const byte SmartsearchMask = 0b00100000;
    const byte LightMask = 0b00010000;

    internal override byte?[] ResponsePattern => new byte?[] { null, 0x00, 0x40, 0x00, 0xef };

    internal override LocomotiveInformation ParseResponseBytes(byte[] response) {
      CheckXORByte(response, 4);

      var speedStepSetting = (SpeedStepSetting)(SpeedStepMask & response[7]);
      return new LocomotiveInformation {
        Address = ByteArrayToShort(response[6], (byte)(response[5] & 0x3f)),
        IsBusy = (LocomotiveBusyMask & response[7]) != 0,
        TrainSpeed = new TrainSpeed(speedStepSetting, response[8]),
        TrainFunctions = (TrainFunctions)ByteArrayToInt(response[9], response[10], response[11], response[12])
      };

    }
  }
}
