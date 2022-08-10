using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using ReactiveUI;
using Track;
using TrainRepository;

namespace TrainTracker {
  public class TrainTracker : IDisposable {
    private CompositeDisposable subscription;
    private readonly List<TrainLocation> trainLocations = new List<TrainLocation>();

    public void Setup(TrackRepository trackRepository) {
      var disposer = new CompositeDisposable();
      foreach (var section in trackRepository.Sections) {
        section.WhenAnyValue(x => x.IsOccupied)
          .DistinctUntilChanged()
          .Subscribe(newValue => {
            foreach(var location in trainLocations) {
              if(location.Update(section, newValue)) {
                Debug.WriteLine($"Updated occupancy {location}");
                return;
              }
            }
          })
          .DisposeWith(disposer);
      }
      subscription = disposer;
    }



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
        .Subscribe(_ => location.TurnAround())
        .DisposeWith(subscription);
    }
  }
}
