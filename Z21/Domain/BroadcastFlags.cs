using System;
using System.Collections.Generic;
using System.Text;

namespace Z21.Domain {
  [Flags]
  public enum BroadcastFlags : int {
    None = 0,
    DrivingAndSwitching = 0x00000001,
    RBus = 0x00000002,
    RailCom = 0x00000004,
    Z21SystemState = 0x00000100,
    AllLocs = 0x00010000,
    LocoNetWithoutLocosAndSwitches = 0x01000000,
    LocoNetSpecific = 0x02000000,
    LogoNetSpecific2 = 0x04000000,
    LocoNetOccupancy = 0x08000000,
    RailComDataChanged = 0x00040000,
    CANBusOccupancy = 0x00080000
  }
}
