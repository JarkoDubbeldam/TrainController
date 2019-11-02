using System;
using System.Collections.Generic;
using System.Text;

namespace Z21.API {
  public abstract class Request {
    internal abstract byte[] ToByteArray();
  }
}
