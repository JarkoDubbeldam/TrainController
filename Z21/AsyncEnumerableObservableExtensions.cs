using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Z21;
public static class AsyncEnumerableObservableExtensions {
  public static IObservable<T> ToObservable<T>(this IAsyncEnumerable<T> values) => Observable.Create<T>(o => {
    var cts = new CancellationTokenSource();
    Task.Run(async () => {
      try {
        await foreach (var element in values.WithCancellation(cts.Token)) {
          o.OnNext(element);
        }
        o.OnCompleted();
      } catch (OperationCanceledException) { 
      } catch (Exception e) {
        o.OnError(e);
      }
      cts.Dispose();
    });
    return () => {
      cts.Cancel();
      cts.Dispose();
    };
  });
}
