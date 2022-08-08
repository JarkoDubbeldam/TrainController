using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using ReactiveUI;
using Track;
using TrainRepository;

namespace TrainTracker {
  public class TrainTracker : IDisposable {
    private IDisposable subscription;
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
    public void AddTrain(Train train, IEnumerable<TrackConnection> sections) => trainLocations.Add(new TrainLocation(train, sections));
  }
}
