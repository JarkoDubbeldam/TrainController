using System;
using System.Linq;

namespace Z21.API {
  public class SerialNumberResponseFactory : ResponseFactory<int> {
    internal override byte?[] ResponsePattern => new byte?[] { 0x08, 0x00, 0x10, 0x00 };

    internal override int ParseResponseBytes(byte[] response) {
      return ByteArrayToInt(response.Skip(4).ToArray());
    }


  }
}