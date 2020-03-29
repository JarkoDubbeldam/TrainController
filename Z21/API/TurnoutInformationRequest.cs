using System;
using System.Collections.Generic;
using System.Text;
using Z21.Domain;

namespace Z21.API {
  public class TurnoutInformationRequest : AddressSpecificRequest<TurnoutInformation> {
    public short Address { get; set; }

    internal override ResponseFactory<TurnoutInformation> GetResponseFactory() {
      return new TurnoutInformationResponseFactory(Address);
    }

    internal override byte[] ToByteArray() {
      Address.GetAddressBytes(out var msb, out var lsb);

      var requestBytes = new byte[] { 0x08, 0x00, 0x40, 0x00, 0x43, msb, lsb, default };
      requestBytes[7] = (byte)(requestBytes[4] ^ requestBytes[5] ^ requestBytes[6]);
      return requestBytes;
    }
  }
}
