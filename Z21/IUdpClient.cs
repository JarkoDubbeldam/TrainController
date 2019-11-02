using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Z21 {
  public interface IUdpClient : IDisposable {
    void SendBytes(byte[] bytes);
    byte[] ReceiveBytes();
  }
}
