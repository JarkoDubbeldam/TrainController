using System;
using System.Collections.Generic;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  public class TrackStatusRequest : RequestWithResponse<TrackStatus> {
    public TrackStatus TrackStatus { get; set; }

    internal override ResponseFactory<TrackStatus> GetResponseFactory() {
      return new TrackStatusResponseFactory();
    }

    internal override byte[] ToByteArray() {
      if (TrackStatus != TrackStatus.On && TrackStatus != TrackStatus.Off) {
        throw new InvalidOperationException($"Can't programmatically set track status to {TrackStatus}.");
      }
      var statusByte = (byte)(0x80 + (byte)TrackStatus);
      return new byte[] {
        0x07, 0x00, 0x40, 0x00, 0x21, statusByte, (byte)(0x21 ^ statusByte)
      };
    }
  }
}
