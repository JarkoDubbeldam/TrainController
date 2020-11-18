using System.Collections;

namespace Z21.Domain {
  public class OccupancyStatus {
    public int GroupIndex { get; internal set; }
    public BitArray Occupancies { get; internal set; }
  }
}