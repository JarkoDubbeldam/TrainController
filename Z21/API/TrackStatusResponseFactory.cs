using System;
using System.Collections.Generic;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  public class TrackStatusResponseFactory : ResponseFactory<TrackStatus> {
    internal override byte?[] ResponsePattern => new byte?[] { 0x07, 0x00, 0x40, 0x00, 0x61 };

    internal override TrackStatus ParseResponseBytes(byte[] response) {
      var status = (TrackStatus)response[5];
      CheckXORByte(response, 4);
      return status;
    }
  }
}
