using System.Collections;
using System.Collections.Generic;

namespace Z21 {
  enum MessageType {
    SerialNumber,
    XGetVersion,
    TrackPowerStatus,
    UnknownCommand,
    StatusChanged,
    BCStopped,
    FirmwareVersion,
    BroadcastFlags,
    SystemState,
    HardwareInfo,
    FeatureCode,
    LocoMode,
    TurnoutMode
  }
}