using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  internal class BroadcastFlagsResponse : ResponseFactory<BroadcastFlags> {
    internal override byte?[] ResponsePattern => new byte?[] { 0x08, 0x00, 0x51 };

    internal override BroadcastFlags ParseResponseBytes(byte[] response) {
      return (BroadcastFlags)ByteArrayToInt(response.Skip(4).ToArray());
    }
  }
}
