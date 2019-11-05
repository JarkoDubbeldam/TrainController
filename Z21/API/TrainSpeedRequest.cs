using System;
using System.Collections.Generic;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  public class TrainSpeedRequest : Request {
    public short TrainAddress { get; set; }
    public TrainSpeed TrainSpeed { get; set; }
    internal override byte[] ToByteArray() {
      var addressBytes = BitConverter.GetBytes(TrainAddress);
      if (!BitConverter.IsLittleEndian) {
        Array.Reverse(addressBytes);
      }

      var speedByte = TrainSpeed.ToSpeedByte();
      var speedSettingByte = NewMethod(TrainSpeed.speedStepSetting);
      var request = new byte[] { 0x0a, 0x00, 0x40, 0x00, 0xe4, speedSettingByte, (byte)(addressBytes[1] | 0xC0), addressBytes[0], speedByte, default };
      request[9] = (byte)(request[4] ^ request[5] ^ request[6] ^ request[7] ^ request[8]);
      return request;
    }

    private static byte NewMethod(SpeedStepSetting speedStepSetting) {
      switch (speedStepSetting) {
        case SpeedStepSetting.Step14:
          return 0x10;
        case SpeedStepSetting.Step28:
          return 0x12;
        case SpeedStepSetting.Step128:
          return 0x13;

        default:
          throw new ArgumentOutOfRangeException(nameof(speedStepSetting));
      }

    }
  }
}
