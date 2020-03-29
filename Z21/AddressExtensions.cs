using System;
using System.Collections.Generic;
using System.Text;

namespace Z21 {
  internal static class AddressExtensions {
    public static void GetAddressBytes(this short address, out byte msb, out byte lsb) {
      var addressBytes = BitConverter.GetBytes(address);
      if (!BitConverter.IsLittleEndian) {
        Array.Reverse(addressBytes);
      }
      msb = addressBytes[1];
      lsb = addressBytes[0];
    }
  }
}
