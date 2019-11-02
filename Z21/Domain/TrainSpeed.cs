using System;
using System.Collections.Generic;

namespace Z21.Domain {
  public struct TrainSpeed {
    private const byte DirectionMask = 0b10000000;
    private const byte Speed128Mask = 0b01111111;
    private const byte Speed14Mask = 0b00001111;
    private const byte ExtraBitSpeed28Mask = 0b00010000;

    public readonly SpeedStepSetting speedStepSetting;
    public readonly DrivingDirection drivingDirection;
    public readonly Speed speed;

    public TrainSpeed(SpeedStepSetting speedStepSetting, DrivingDirection drivingDirection, Speed speed) {
      if(speed > MaxSpeeds[speedStepSetting]) {
        throw new ArgumentOutOfRangeException(nameof(speed), $"Max speed with setting {speedStepSetting} is {MaxSpeeds[speedStepSetting]}, actual {speed}");
      }
      if(speed < Speed.Stop) {
        throw new ArgumentOutOfRangeException(nameof(speed), $"Min speed is -1 as {Speed.Stop}, to change direction, use {nameof(drivingDirection)}.");
      }

      this.speedStepSetting = speedStepSetting;
      this.drivingDirection = drivingDirection;
      this.speed = speed;
    }

    internal TrainSpeed(SpeedStepSetting speedStepSetting, byte speedByte) {
      this.speedStepSetting = speedStepSetting;
      this.drivingDirection = (speedByte & DirectionMask) == 0 ? DrivingDirection.Backward : DrivingDirection.Forward;
      if(speedStepSetting == SpeedStepSetting.Step128) {
        this.speed = (Speed)((Speed128Mask & speedByte) - 1);
      } else {
        this.speed = (Speed)((Speed14Mask & speedByte) - 1);
        if(speed > 0 && speedStepSetting == SpeedStepSetting.Step28) {
          this.speed = (Speed)(((sbyte)speed << 1) - ((speedByte & ExtraBitSpeed28Mask) == 0 ? 1 : 0));
        }
      }
    }

    internal byte ToSpeedByte() {
      var directionByte = ((byte)drivingDirection << 7);
      if(speedStepSetting == SpeedStepSetting.Step128 || speed <= 0) {
        return (byte)(directionByte + (byte)(speed + 1));
      } else {
        var resultByte = directionByte;
        var speed = (int)this.speed;
        if(speedStepSetting == SpeedStepSetting.Step28) {
          resultByte |= speed % 2 == 0 ? 0b00010000 : 0;
          speed /= 2;
        }
        resultByte |= speed + 1;
        return (byte)resultByte;
      }
    }

    public override string ToString() {
      return $"Direction: {drivingDirection}, Speed: {speed}";
    }

    private static readonly Dictionary<SpeedStepSetting, Speed> MaxSpeeds = new Dictionary<SpeedStepSetting, Speed> { { SpeedStepSetting.Step14, (Speed)14 }, { SpeedStepSetting.Step28, (Speed)28 }, { SpeedStepSetting.Step128, (Speed)126 } }; 
  }
  public enum SpeedStepSetting : byte {
    Step14 = 0,
    Step28 = 2,
    Step128 = 4
  }

  public enum DrivingDirection : byte {
    Backward = 0,
    Forward = 1
  }

  public enum Speed : sbyte {
    Stop = -1,
    EmergencyStop = 0
  }
}
