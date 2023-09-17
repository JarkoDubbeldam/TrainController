using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Z21.API;

namespace Z21;
internal class RequestResponseConnector {
  private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(1);
  private readonly Func<IUdpClient> clientFactory;

  public RequestResponseConnector(Func<IUdpClient> clientFactory) {
    this.clientFactory = clientFactory;
  }

  public async Task<TResponse> Execute<TResponse>(RequestWithResponse<TResponse> request) {
    using var client = clientFactory();
    var responseFactory = request.GetResponseFactory();
    var timeoutDue = DateTimeOffset.Now + _timeout;
    try {
      client.SendBytes(request.ToByteArray());
      return await client.ObserveBytes()
        .SingleAsync(x => x.MatchesPattern(responseFactory.ResponsePattern))
        .Select(x => responseFactory.ParseResponseBytes(x))
        .Timeout(timeoutDue);
    } finally {
      client.SendBytes(new LogOffRequest().ToByteArray());
    }
  }
}
