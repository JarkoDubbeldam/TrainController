using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Z21.API;
using Z21.Domain;

namespace Z21;
internal class UpdateStreamConnection<TUpdate> : IDisposable {
  private readonly IUdpClient udpClient;
  private readonly Subject<Unit> disposed = new();
  private readonly IObservable<TUpdate> updateObservable;
  private bool isDisposed = false;

  private UpdateStreamConnection(IUdpClient udpClient, ResponseFactory<TUpdate> responseFactory) {
    this.udpClient = udpClient;
    updateObservable = udpClient.ObserveBytes()
      .Where(x => x.MatchesPattern(responseFactory.ResponsePattern))
      .Select(x => responseFactory.ParseResponseBytes(x))
      .TakeUntil(disposed)
      .Publish()
      .AutoConnect();

    var _ = Observable.Timer(TimeSpan.FromSeconds(50))
      .Do(_ => udpClient.SendBytes(new SerialNumberRequest().ToByteArray()))
      .TakeUntil(disposed)
      .Subscribe();
  }


  public IObservable<TUpdate> UpdateObservable => isDisposed ? throw new ObjectDisposedException(nameof(UpdateStreamConnection<TUpdate>)) : updateObservable;
  public void Dispose() {
    disposed.OnNext(Unit.Default);
    isDisposed = true;
    disposed.Dispose();
    udpClient.Dispose();
  }

  public static UpdateStreamConnection<TUpdate> CreateUpdateStream<TFactory>(BroadcastFlags broadcastFlags, Func<IUdpClient> clientFactory)
    where TFactory : ResponseFactory<TUpdate>, new() 
    => CreateUpdateStream(new TFactory(), broadcastFlags, clientFactory);

  public static UpdateStreamConnection<TUpdate> CreateUpdateStream(ResponseFactory<TUpdate> responseFactory, BroadcastFlags broadcastFlags, Func<IUdpClient> clientFactory) {
    var client = clientFactory();
    var broadcastFlagRequest = new SetBroadcastFlagsRequest { BroadcastFlags = broadcastFlags };
    client.SendBytes(broadcastFlagRequest.ToByteArray());

    return new UpdateStreamConnection<TUpdate>(client, responseFactory);
  }
}
