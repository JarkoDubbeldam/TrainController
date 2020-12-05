using System;
using System.Collections.Generic;
using System.Linq;
using Z21.Domain;

namespace Z21.API {
  public class TrainFunctionRequest : Request {
    public short TrainAddress { get; set; }
    public TrainFunctions TrainFunctions { get; set; }
    public TrainFunctionToggleMode TrainFunctionToggleMode { get; set; }

    internal override byte[] ToByteArray() {
      var addressBytes = BitConverter.GetBytes(TrainAddress);
      if (!BitConverter.IsLittleEndian) {
        Array.Reverse(addressBytes);
      }
      if (!TrainFunctionOrder.Contains(TrainFunctions)) {
        throw new ArgumentException("TrainFunctions can only contain a single flag.", nameof(TrainFunctions));
      }

      var request = new byte[] { 0x0a, 0x00, 0x40, 0x00, 0xe4, 0xf8, (byte)(addressBytes[1] | 0xC0), addressBytes[0], default, default };
      request[8] = (byte)((byte)TrainFunctionToggleMode | GetFunctionIndex(TrainFunctions));
      request[9] = (byte)(request[4] ^ request[5] ^ request[6] ^ request[7] ^ request[8]);
      return request;

    }

    private static int GetFunctionIndex(TrainFunctions trainFunction) {
      var index = TrainFunctionOrder.IndexOf(trainFunction);

      if (index < 0) {
        throw new ArgumentOutOfRangeException(nameof(trainFunction));
      }

      return index;
    }

    private static readonly List<TrainFunctions> TrainFunctionOrder = new List<TrainFunctions>{
      TrainFunctions.Lights,
      TrainFunctions.Function1,
      TrainFunctions.Function2,
      TrainFunctions.Function3,
      TrainFunctions.Function4,
      TrainFunctions.Function5,
      TrainFunctions.Function6,
      TrainFunctions.Function7,
      TrainFunctions.Function8,
      TrainFunctions.Function9,
      TrainFunctions.Function10,
      TrainFunctions.Function11,
      TrainFunctions.Function12,
      TrainFunctions.Function13,
      TrainFunctions.Function14,
      TrainFunctions.Function15,
      TrainFunctions.Function16,
      TrainFunctions.Function17,
      TrainFunctions.Function18,
      TrainFunctions.Function19,
      TrainFunctions.Function20,
      TrainFunctions.Function21,
      TrainFunctions.Function22,
      TrainFunctions.Function23,
      TrainFunctions.Function24,
      TrainFunctions.Function25,
      TrainFunctions.Function26,
      TrainFunctions.Function27,
      TrainFunctions.Function28
    };
  }
}