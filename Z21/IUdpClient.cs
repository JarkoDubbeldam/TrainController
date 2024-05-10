using System;

namespace Z21 {
  public interface IUdpClient : IDisposable {
    void SendBytes(byte[] bytes);
    IObservable<byte[]> ObserveBytes();
  }
}
