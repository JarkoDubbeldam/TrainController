using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Z21;

namespace Track;
public sealed class TrackRepositoryFactory : IDisposable {
  private readonly IZ21Client client;
  private readonly ILogger<SignalConfiguration> logger;
  private readonly ConcurrentDictionary<string, TrackRepository> cache = new();

  public TrackRepositoryFactory(IZ21Client client, ILogger<SignalConfiguration> logger) {
    this.client = client;
    this.logger = logger;
  }

  public TrackRepository Build(string json) {
    return cache.GetOrAdd(json, s => TrackRepository.FromJson(json, client, logger));
  }

  public TrackRepository GetAny() {
    return cache.Values.First();
  }

  public void Dispose() {
    foreach (var repos in cache.Values) {
      repos.Dispose();
    }
  }
}
