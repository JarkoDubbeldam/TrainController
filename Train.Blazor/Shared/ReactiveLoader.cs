using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Trains.Blazor.Shared;

public class ReactiveLoader : IDisposable {
  private readonly Subject<Unit> disposed = new();
  private readonly Subject<Unit> reload = new();
  private readonly Subject<bool> loading = new();
  private readonly Func<CancellationToken, Task> func;

  public ReactiveLoader(IObservable<Unit> observable, Func<CancellationToken, Task> func) {
    if (observable is null) {
      throw new ArgumentNullException(nameof(observable));
    }

    this.func = func ?? throw new ArgumentNullException(nameof(func));

    observable.Merge(reload).Select(_ => Observable.FromAsync(Load)).Switch().TakeUntil(disposed).Subscribe();
  }

  public IObservable<bool> Loading => loading;

  public void Dispose() {
    disposed.OnNext(Unit.Default);
  }

  public void Reload() {
    reload.OnNext(Unit.Default);
  }

  private Task Load(CancellationToken cancellationToken) {
    try {
      loading.OnNext(true);
      return func(cancellationToken);
    } finally {
      loading.OnNext(false);
    }
  }
}
