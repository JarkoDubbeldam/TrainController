using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Z21 {
  internal abstract class Promise<TInput> {
    public Promise(CancellationToken token) {
      token.Register(SetFaulted);
      tcs = new TaskCompletionSource<TInput>(token);
    }

    private readonly TaskCompletionSource<TInput> tcs;

    public Task<TInput> Task => tcs.Task;


    private void SetFaulted() {
      tcs.TrySetException(new TimeoutException());
    }
    public void RecieveMessageEventHandler(object sender, TInput message) {
      if (Matches(message)) {
        tcs.SetResult(message);
      }
    }

    private protected abstract bool Matches(TInput input);
  }
}
