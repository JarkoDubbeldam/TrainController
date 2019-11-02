using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Z21.API {
  public abstract class ResponseFactory<T> {
    internal abstract byte?[] ResponsePattern { get; }
    internal abstract T ParseResponseBytes(byte[] response);

    protected static int ByteArrayToInt(params byte[] response) {
      if (BitConverter.IsLittleEndian) {
        return BitConverter.ToInt32(response);
      } else {
        return BitConverter.ToInt32(response.Reverse().ToArray(), 0);
      }
    }

    protected static short ByteArrayToShort(params byte[] response) {
      if (BitConverter.IsLittleEndian) {
        return BitConverter.ToInt16(response);
      } else {
        return BitConverter.ToInt16(response.Reverse().ToArray(), 0);
      }
    }

    protected static ushort ByteArrayToUshort(params byte[] response) {
      if (BitConverter.IsLittleEndian) {
        return BitConverter.ToUInt16(response);
      } else {
        return BitConverter.ToUInt16(response.Reverse().ToArray(), 0);
      }
    }
  }
}
