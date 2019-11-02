using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  public class SetBroadcastFlagsRequest : Request {
    public BroadcastFlags BroadcastFlags { get; set; }

    internal override byte[] ToByteArray() {
      IEnumerable<byte> flagBits = BitConverter.GetBytes((int)BroadcastFlags);
      if (!BitConverter.IsLittleEndian) {
        flagBits = flagBits.Reverse();
      }
      return new byte[] { 0x08, 0x00, 0x50, 0x00 }.Concat(flagBits).ToArray();
    }
  }


}
