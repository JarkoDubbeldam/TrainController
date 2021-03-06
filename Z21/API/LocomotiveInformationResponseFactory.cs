﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  internal class LocomotiveInformationResponseFactory : ResponseFactory<LocomotiveInformation> {
    public LocomotiveInformationResponseFactory() {
      responsePattern = BuildResponsePattern();
    }

    public LocomotiveInformationResponseFactory(short address) {
      address.GetAddressBytes(out var msb, out var lsb);
      responsePattern = BuildResponsePattern(msb, lsb);
    }


    private static byte?[] BuildResponsePattern(params byte?[] addressBytes) {
      return new byte?[] { null, 0x00, 0x40, 0x00, 0xef }.Concat(addressBytes).ToArray();
    }

    const byte LocomotiveBusyMask = 0b00001000;
    const byte SpeedStepMask = 0b00000111;
    const byte DoubleTractionMask = 0b0100000;
    const byte SmartsearchMask = 0b00100000;
    const byte LightMask = 0b00010000;
    private readonly byte?[] responsePattern;

    internal override byte?[] ResponsePattern => responsePattern;

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
