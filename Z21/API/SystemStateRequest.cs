using System;
using System.Collections.Generic;
using System.Text;

namespace Z21.API {
  public class SystemStateRequest : Request {
    internal override byte[] ToByteArray() {
      return new byte[] { 0x04, 0x00, 0x85, 0x00 };
    }
  }
}
