using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ReactiveUI;
using Track;
using TrainRepository;

namespace TrainTracker;
public sealed class TrainTracker : IDisposable {
  private CompositeDisposable subscription;
  private readonly List<TrainLocation> trainLocations = new();
  private readonly ILogger<TrainTracker> logger;

  public TrainTracker(ILogger<TrainTracker> logger) {
    this.logger = logger;
  }

  public void Setup(TrackRepository trackRepository) {
    var disposer = new CompositeDisposable();
    foreach (var section in trackRepository.Sections) {
      section.WhenAnyValue(x => x.IsOccupied)
        .DistinctUntilChanged()
        .Subscribe(newValue => {
          foreach (var location in trainLocations) {
            if (location.Update(section, newValue)) {
              logger.LogInformation($"Updated occupancy {location}");
              return;
            }
          }
        })
        .DisposeWith(disposer);
    }
    subscription = disposer;
  }

  internal string Serialize() => JsonConvert.SerializeObject(trainLocations.Select(x => new {
    TrainId = x.Train.Address,
    TrainName = x.Train.Name,
    SectionIds = x.OccupiedConnections.Select(y => y.ViaSection.SectionId).ToHashSet()
  }));

  public void Dispose() => subscription?.Dispose();
  public void AddTrain(Train train, IEnumerable<TrackConnection> sections) {
    var location = new TrainLocation(train, sections);
    trainLocations.Add(location);
    Observable.FromEvent<PropertyChangedEventHandler, Train>(
      handler => (sender, e) => handler((Train)sender),
      handler => train.PropertyChanged += handler,
      handler => train.PropertyChanged -= handler
    )
      .Select(x => x.Speed.DrivingDirection)
      .DistinctUntilChanged()
      .Subscribe(_ => {
        location.TurnAround();
        logger.LogInformation("Train {train} turned around.", train.Name);
      })
      .DisposeWith(subscription);
  }
}
