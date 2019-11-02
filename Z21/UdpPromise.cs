using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Z21 {
  internal class UdpPromise : Promise<byte[]> {
    private readonly byte?[] matchPattern;
    public UdpPromise(byte?[] matchPattern) {
      this.matchPattern = matchPattern;
    }

    private protected override bool Matches(byte[] message) {
      return message.Zip(matchPattern, (m, p) => p == null || p == m).All(x => x);
    }
  }
}
