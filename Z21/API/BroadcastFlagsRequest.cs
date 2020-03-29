using System;
using System.Collections.Generic;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  public class BroadcastFlagsRequest : RequestWithResponse<BroadcastFlags> {
    internal override ResponseFactory<BroadcastFlags> GetResponseFactory() {
      return new BroadcastFlagsResponseFactory();
    }

    internal override byte[] ToByteArray() => new byte[] { 0x04, 0x00, 0x51, 0x00 };
  }
}
