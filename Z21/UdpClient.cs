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
using SysClient = System.Net.Sockets.UdpClient;

namespace Z21 {
  public sealed class UdpClient : IUdpClient, IDisposable {
    private readonly SysClient sysClient;
    private readonly IPEndPoint endpoint;
    private readonly ILogger<UdpClient> logger;
    private readonly IObservable<byte[]> instream;
    private IDisposable? instreamDisposable;

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
      logger.LogDebug("Starting listen");
      var result = await sysClient.ReceiveAsync(cancellationToken);
      logger.LogDebug("Received message {bytes}", string.Join(" ", result.Buffer.Select(x => x.ToString())));
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
      logger.LogDebug("Sent {message}", string.Join(' ', bytes.Select(x => x.ToString())));
      sysClient.Send(bytes, bytes.Length, endpoint);
    }

    public void Dispose() {
      instreamDisposable?.Dispose();
      sysClient.Dispose();
    }
  }
}
