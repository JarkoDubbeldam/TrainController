using System;

using Z21.Domain;

namespace Z21.API {
  public class OccupancyStatusRequest : RequestWithResponse<OccupancyStatus> {
    public int GroupIndex { get; set; }
    internal override ResponseFactory<OccupancyStatus> GetResponseFactory() => new OccupancyStatusResponseFactory((byte)GroupIndex);

    internal override byte[] ToByteArray() {
      if (GroupIndex != 0 && GroupIndex != 1) {
        throw new ArgumentOutOfRangeException(nameof(GroupIndex));
      }

      return new byte[] { 0x05, 0x00, 0x81, 0x00, (byte)GroupIndex };
    }
  }
}