using System;
using System.Collections.Generic;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  public class TrackStatusResponse : ResponseFactory<TrackStatus> {
    internal override byte?[] ResponsePattern => new byte?[] { 0x07, 0x00, 0x40, 0x00, 0x61 };

    internal override TrackStatus ParseResponseBytes(byte[] response) {
      var status = (TrackStatus)response[5];
      if((response[4] ^ response[5]) != response[6]) {
        throw new ArgumentException();
      }
      return status;
    }
  }
}
