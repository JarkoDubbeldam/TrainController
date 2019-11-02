using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SysClient = System.Net.Sockets.UdpClient;

namespace Z21 {
  public class UdpClient : IUdpClient {
    private readonly SysClient sysClient;
    private IPEndPoint endpoint;

    public UdpClient(SysClient sysClient, IPEndPoint endpoint) {
      this.sysClient = sysClient;
      this.endpoint = endpoint;
    }



    public byte[] ReceiveBytes() {
      return sysClient.Receive(ref endpoint);
    }

    public void SendBytes(byte[] bytes) {
      sysClient.Send(bytes, bytes.Length, endpoint);
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!disposedValue) {
        if (disposing) {
          sysClient.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        disposedValue = true;
      }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~UdpClient()
    // {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose() {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion
  }
}
