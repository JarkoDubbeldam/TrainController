using System;
using System.Collections.Generic;
using System.Text;

namespace Z21.API {
  public class LocomotiveInformationRequest : Request {
    public short LocomotiveAddress { get; set; }

    internal override byte[] ToByteArray() {
      var addressBytes = GetAddressBytes();

      var bytes = new byte[9] {
        0x09,
        0x00,
        0x40,
        0x00,
        0xe3,
        0xf0,
        (byte)(0xc0 | addressBytes[1]),
        addressBytes[0],
        default
      };
      bytes[8] = (byte)(bytes[4] ^ bytes[5] ^ bytes[6] ^ bytes[7]);

      return bytes;
    }

    public byte[] GetAddressBytes() {
      var addressBytes = BitConverter.GetBytes(LocomotiveAddress);
      if (!BitConverter.IsLittleEndian) {
        Array.Reverse(addressBytes);
      }

      return addressBytes;
    }
  }
}
