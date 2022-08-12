using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Track;
using TrainRepository;

namespace TrainTracker;
public sealed class TrainTrackerFactory : IAsyncDisposable {
  private readonly ILifetimeScope lifetimeScope;
  private readonly ConcurrentDictionary<string, Task<TrainTracker>> cache = new();

  public TrainTrackerFactory(ILifetimeScope lifetimeScope) {
    this.lifetimeScope = lifetimeScope;
  }

  public Task<TrainTracker> Build(string fileName) => cache.GetOrAdd(fileName, async f => {
    var json = await File.ReadAllTextAsync(f);
    var tracker = new TrainTracker(lifetimeScope.Resolve<ILogger<TrainTracker>>());
    var data = JsonConvert.DeserializeObject<IEnumerable<JsonData>>(json);
    var repository = lifetimeScope.Resolve<IRepository<Train>>();
    var track = lifetimeScope.Resolve<TrackRepository>();
    foreach (var trackedTrain in data) {
      var train = await repository.RegisterObject(trackedTrain.TrainId, trackedTrain.TrainName);
      var sections = track.Boundaries
        .SelectMany(x => x.Connections)
        .Distinct()
        .Where(x => trackedTrain.SectionIds.Contains(x.ViaSection.SectionId));
      tracker.AddTrain(train, sections);
    }
    tracker.Setup(track);
    return tracker;
  });
  public async ValueTask DisposeAsync() {
    foreach(var element in cache) {
      await File.WriteAllTextAsync(element.Key, (await element.Value).Serialize());
      (await element.Value).Dispose();
    }
  }

  private class JsonData {
    public int TrainId { get; set; }
    public string TrainName { get; set; }
    public List<int> SectionIds { get; set; }
  }
}
