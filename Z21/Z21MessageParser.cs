using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Z21 {
  internal class Z21MessageParser {
    public IEnumerable<(MessageType messageType, byte[] messageData)> ParseZ21Message(byte[] udpDataBytes) {
      var index = 0;
      for(; index < udpDataBytes.Length;) {
        var messageLength = udpDataBytes[index] + (udpDataBytes[index + 1] << 4);
        index += 2;
        var header = udpDataBytes[index] + (udpDataBytes[index + 1] << 4);
        index += 2;
        var remainingLength = messageLength - 4;
        var messageData = udpDataBytes.Skip(index).Take(remainingLength).ToArray();
        var messageType = ParseMessageType(header, messageData);
        yield return (messageType, messageData);
        index += messageLength;
      }
    }

    private MessageType ParseMessageType(int header, byte[] messageData) {
      switch (header) {
        case 0x10:
          return MessageType.SerialNumber;

        // X-Bus group
        case 0x40:
          return ParseXBusMessageType(messageData);

        case 0x51:
          return MessageType.BroadcastFlags;

        case 0x84:
          return MessageType.SystemState;

        case 0x1A:
          return MessageType.HardwareInfo;

        case 0x18:
          return MessageType.FeatureCode;

        case 0x60:
          return MessageType.LocoMode;

        case 0x70:
          return MessageType.TurnoutMode;
      }
      throw new NotImplementedException();
    }

    private MessageType ParseXBusMessageType(byte[] messageData) {
      switch (messageData[0]) {
        case 0x63:
          return MessageType.XGetVersion;

        case 0x61:
          if(messageData[1] == 0x82) {
            return MessageType.UnknownCommand;
          }
          return MessageType.TrackPowerStatus;

        case 0x62:
          return MessageType.StatusChanged;

        case 0x81:
          return MessageType.BCStopped;

        case 0xF3:
          return MessageType.FirmwareVersion;


      }
      throw new NotImplementedException();
    }
  }
}
