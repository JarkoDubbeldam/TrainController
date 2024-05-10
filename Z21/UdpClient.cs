using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Z21.API;
using SysClient = System.Net.Sockets.UdpClient;

namespace Z21 {
  public sealed class UdpClient : IUdpClient, IDisposable {
    private readonly SysClient sysClient;
    private readonly IPEndPoint endpoint;
    private readonly ILogger<UdpClient> logger;
    private readonly IObservable<byte[]> instream;
    private IDisposable? instreamDisposable;
    private bool disposedValue;

    public UdpClient(IOptions<Z21Settings> options, ILogger<UdpClient> logger) {
      this.sysClient = new SysClient();
      this.endpoint = options.Value.Z21Endpoint;
      this.logger = logger;
      this.instream = Observable.FromAsync(ListenAsync)
        .Repeat()
        .Where(x => x.RemoteEndPoint == endpoint)
        .SelectMany(x => SplitMessages(x.Buffer))
        .Publish()
        .AutoConnect(onConnect: d => instreamDisposable = d);
    }

    private async Task<UdpReceiveResult> ListenAsync(CancellationToken cancellationToken) {
      logger.LogInformation("Starting listen");
      var result = await sysClient.ReceiveAsync(cancellationToken);
      logger.LogInformation("Receive {fromip} -> {toip}: {bytes}", result.RemoteEndPoint, sysClient.Client.LocalEndPoint, string.Join(" ", result.Buffer.Select(x => x.ToString())));
      return result;
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
      sysClient.Send(bytes, bytes.Length, endpoint);
      logger.LogInformation("Sent {fromip} -> {toip}: {bytes}", sysClient.Client.LocalEndPoint, endpoint, string.Join(" ", bytes.Select(x => x.ToString())));
    }

    private void Dispose(bool disposing) {
      if (!disposedValue) {
        if (disposing) {
          instreamDisposable?.Dispose();

          this.SendBytes(new LogOffRequest().ToByteArray());
          sysClient.Dispose();
        }
        disposedValue = true;
      }
    }

    public void Dispose() {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
