using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SysClient = System.Net.Sockets.UdpClient;

namespace Z21 {
  public class UdpClient : IUdpClient {
    private readonly SysClient sysClient;
    private IPEndPoint endpoint;
    private readonly ILogger<UdpClient> logger;
    private readonly UdpObservable listener;
    private readonly IObservable<byte[]> instream;

    public UdpClient(SysClient sysClient, IPEndPoint endpoint, ILogger<UdpClient> logger) {
      this.sysClient = sysClient;
      this.endpoint = endpoint;
      this.logger = logger;
      listener = new UdpObservable(sysClient, logger);
      instream = listener.SelectMany(SplitMessages);
    }


    public IObservable<byte[]> ObserveBytes() => instream;

    private IEnumerable<byte[]> SplitMessages(byte[] message) {
      var index = 0;
      while (index < message.Length) {
        var messageLength = message[index];
        var subMessage = message.Skip(index).Take(messageLength).ToArray();
        yield return subMessage;
        index += messageLength;
      }
    }

    public void SendBytes(byte[] bytes) {
      logger.LogInformation($"Sent {string.Join(' ', bytes.Select(x => x.ToString()))}");
      sysClient.Send(bytes, bytes.Length, endpoint);
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!disposedValue) {
        if (disposing) {
          sysClient.Dispose();
          listener.Dispose();
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
