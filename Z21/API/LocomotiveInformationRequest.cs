using System;
using System.Collections.Generic;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  public class LocomotiveInformationRequest : AddressSpecificRequest<LocomotiveInformation> {
    public short LocomotiveAddress { get; set; }

    internal override ResponseFactory<LocomotiveInformation> GetResponseFactory() {
      return new LocomotiveInformationResponseFactory(LocomotiveAddress);
    }

    internal override byte[] ToByteArray() {
      LocomotiveAddress.GetAddressBytes(out var msb, out var lsb);

      var bytes = new byte[9] {
        0x09,
        0x00,
        0x40,
        0x00,
        0xe3,
        0xf0,
        (byte)(0xc0 | msb),
        lsb,
        default
      };
      bytes[8] = (byte)(bytes[4] ^ bytes[5] ^ bytes[6] ^ bytes[7]);

      return bytes;
    }


  }
}
