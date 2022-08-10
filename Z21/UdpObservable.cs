using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SysClient = System.Net.Sockets.UdpClient;

namespace Z21 {
  internal class UdpObservable : IObservable<byte[]>, IDisposable {
    private readonly SysClient sysClient;
    private readonly ILogger<UdpClient> logger;
    private readonly ConcurrentDictionary<IObserver<byte[]>, IDisposable> observers = new ConcurrentDictionary<IObserver<byte[]>, IDisposable>();
    private readonly CancellationTokenSource cts;
    private readonly Task listenTask;
    private bool disposedValue;

    public UdpObservable(SysClient sysClient, ILogger<UdpClient> logger) {
      this.sysClient = sysClient;
      this.logger = logger;
      cts = new CancellationTokenSource();
      listenTask = Task.Run(() => RunLoop(cts.Token), cts.Token);
    }

    public IDisposable Subscribe(IObserver<byte[]> observer) {
      return observers.GetOrAdd(observer, (o) => new UnSubToken(o, this));
    }

    private async Task RunLoop(CancellationToken cancellationToken) {
      var number = 1;
      while (!cancellationToken.IsCancellationRequested) {
        logger.LogInformation($"{number}: Starting Listening");
        var message = await sysClient.ReceiveAsync();
        var x = number;
        _ = Task.Run(() => {
          logger.LogInformation($"{x}: Received {string.Join(' ', message.Buffer.Select(x => x.ToString()))}");
          foreach (var observer in observers.Keys) {
            observer.OnNext(message.Buffer);
          }
        });
        number++;
      }
    }

    private class UnSubToken : IDisposable {
      private readonly IObserver<byte[]> observer;
      private readonly UdpObservable daddy;
      private bool disposedValue;

      public UnSubToken(IObserver<byte[]> observer, UdpObservable daddy) {
        this.observer = observer;
        this.daddy = daddy;
      }

      protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
          if (disposing) {
            daddy.observers.TryRemove(observer, out _);
            // TODO: dispose managed state (managed objects)
          }

          // TODO: free unmanaged resources (unmanaged objects) and override finalizer
          // TODO: set large fields to null
          disposedValue = true;
        }
      }

      // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
      // ~UnSubToken()
      // {
      //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      //     Dispose(disposing: false);
      // }

      public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
      }
    }

    protected virtual void Dispose(bool disposing) {
      if (!disposedValue) {
        if (disposing) {
          cts.Cancel();
          cts.Dispose();
          foreach (var observer in observers.Keys) {
            observer.OnCompleted();
          }
          // TODO: dispose managed state (managed objects)
        }
        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        disposedValue = true;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~UdpObservable()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose() {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
