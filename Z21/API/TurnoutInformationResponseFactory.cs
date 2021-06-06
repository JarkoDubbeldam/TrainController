using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  internal class TurnoutInformationResponseFactory : ResponseFactory<TurnoutInformation> {
    private readonly byte?[] responsePattern;


    public TurnoutInformationResponseFactory() {
      responsePattern = BuildResponsePattern();
    }

    public TurnoutInformationResponseFactory(short address) {
      address.GetAddressBytes(out var msb, out var lsb);
      responsePattern = BuildResponsePattern(msb, lsb);
    }


    private static byte?[] BuildResponsePattern(params byte?[] addressBytes) {
      return new byte?[] { 0x09, 0x00, 0x40, 0x00, 0x43, }.Concat(addressBytes).ToArray();
    }

    internal override byte?[] ResponsePattern => responsePattern;

    internal override TurnoutInformation ParseResponseBytes(byte[] response) {
      CheckXORByte(response, 4);

      var address = ByteArrayToShort(response[6], response[5]);
      var position = (TurnoutPosition)response[7];

      return new TurnoutInformation {
        Address = address,
        TurnoutPosition = position
      };
    }
  }
}
