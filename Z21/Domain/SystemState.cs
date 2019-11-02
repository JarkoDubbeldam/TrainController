using System;
using System.Collections.Generic;
using System.Text;

namespace Z21.Domain {
  public class SystemState {
    public int MainCurrent { get; internal set; }
    public int ProgCurrent { get; internal set; }
    public int FilteredMainCurrent { get; internal set; }
    public int Temperature { get; internal set; }
    public ushort SupplyVoltage { get; internal set; }
    public ushort VCCVoltage { get; internal set; }
    public CentralState CentralState { get; internal set; }
    public CentralStateEx CentralStateEx { get; internal set; }
  }

  [Flags]
  public enum CentralState : byte {
    EmergencyStop = 0x01,
    TrackVoltageOff = 0x02,
    ShortCircuit = 0x04,
    ProgrammingModeActive = 0x20
  }

  [Flags]
  public enum CentralStateEx : byte {
    HighTemperature = 0x01,
    PowerLost = 0x02,
    ShortCircuitExternal = 0x04,
    ShortCircuitInternal = 0x08
  }
}
