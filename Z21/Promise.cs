using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Z21 {
  internal abstract class Promise<TInput> {
    private readonly TaskCompletionSource<TInput> tcs = new TaskCompletionSource<TInput>();

    public Task<TInput> Task => tcs.Task;

    public void RecieveMessageEventHandler(object sender, TInput message) {
      if (Matches(message)) {
        tcs.SetResult(message);
      }
    }

    private protected abstract bool Matches(TInput input);
  }
}
