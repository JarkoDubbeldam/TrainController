using System;
using System.Collections.Generic;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  internal class SystemStateResponseFactory : ResponseFactory<SystemState> {
    internal override byte?[] ResponsePattern => new byte?[] { 0x14, 0x00, 0x84, 0x00 };

    internal override SystemState ParseResponseBytes(byte[] response) {
      return new SystemState {
        MainCurrent = ByteArrayToShort(response[4], response[5]),
        ProgCurrent = ByteArrayToShort(response[6], response[7]),
        FilteredMainCurrent = ByteArrayToShort(response[8], response[9]),
        Temperature = ByteArrayToShort(response[10], response[11]),
        SupplyVoltage = ByteArrayToUshort(response[12], response[13]),
        VCCVoltage = ByteArrayToUshort(response[14], response[15]),
        CentralState = (CentralState)response[16],
        CentralStateEx = (CentralStateEx)response[17]
      };
    }
  }

 
}
