using System.Collections;

namespace Z21.Domain {
  public class OccupancyStatus {
    public int GroupIndex { get; set; }
    public required BitArray Occupancies { get; set; }
  }
}
