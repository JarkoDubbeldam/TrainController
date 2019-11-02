using System;
using System.Collections.Generic;
using System.Text;

namespace Z21.API {
  public class SerialNumberRequest : Request {
    internal override byte[] ToByteArray() => new byte[] { 0x04, 0x00, 0x10, 0x00 };
  }
}
