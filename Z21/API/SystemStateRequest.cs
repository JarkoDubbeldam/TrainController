using System;
using System.Collections.Generic;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  public class SystemStateRequest : RequestWithResponse<SystemState> {
    internal override ResponseFactory<SystemState> GetResponseFactory() {
      return new SystemStateResponseFactory();
    }

    internal override byte[] ToByteArray() {
      return new byte[] { 0x04, 0x00, 0x85, 0x00 };
    }
  }
}
