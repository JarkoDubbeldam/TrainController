using System.Collections;
using System.Linq;

using Z21.Domain;

namespace Z21.API {
  internal class OccupancyStatusResponseFactory : ResponseFactory<OccupancyStatus> {
    private readonly byte? groupIndex;

    public OccupancyStatusResponseFactory(byte? groupIndex = null) {
      this.groupIndex = groupIndex;
    }

    internal override byte?[] ResponsePattern => new byte?[] { 0x0f, 0x00, 0x80, 0x00, groupIndex };

    internal override OccupancyStatus ParseResponseBytes(byte[] response) => new OccupancyStatus {
      Occupancies = new BitArray(response.Skip(5).ToArray()),
      GroupIndex = response[4]
    };    
  }
}