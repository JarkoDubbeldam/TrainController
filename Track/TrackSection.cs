using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using ReactiveUI;

using Z21;
using Z21.API;
using Z21.Domain;

namespace Track {
  [DebuggerDisplay("({SectionId})")]
  public class TrackSection : ReactiveObject {
    private readonly List<TrackSectionBoundary> connectedBoundaries = new List<TrackSectionBoundary>();
    private bool isActive = true;
    private bool isOccupied;

    public int Id { get; internal set; }
    public int SectionId { get; internal set; }
    public IReadOnlyList<TurnoutConfiguration> Turnouts { get; internal set; }
    public IReadOnlyList<TrackSectionBoundary> ConnectedBoundaries { get => connectedBoundaries; }

    public bool IsActive { get => isActive; private set => this.RaiseAndSetIfChanged(ref isActive, value); }
    public bool IsOccupied { get => isOccupied; internal set => this.RaiseAndSetIfChanged(ref isOccupied, value); }

    public void Activate(IZ21Client client) => Observable.Interval(TimeSpan.FromMilliseconds(200))
      .Take(Turnouts.Count)
      .Zip(Turnouts)
      .SelectMany(x => client.SetTurnout(new SetTurnoutRequest {
        Address = (short)x.Second.TurnoutId,
        TurnoutPosition = x.Second.TurnoutMode switch {
          TurnoutMode.Right => TurnoutPosition.Position1,
          TurnoutMode.Left => TurnoutPosition.Position2,
          _ => throw new ArgumentOutOfRangeException()
        },
        Activation = Activation.Activate,
        QueueMode = true
      }))
      .Subscribe();

    internal void AddTrackSectionBoundary(TrackSectionBoundary trackSectionBoundary) => connectedBoundaries.Add(trackSectionBoundary);

    internal IDisposable SetupListener() {
      var d = new CompositeDisposable();
      foreach (var turnout in Turnouts) {
        turnout.WhenAnyValue(t => t.IsActive)
          .DistinctUntilChanged()
          .Subscribe(x => CheckActive())
          .DisposeWith(d);
      }
      return d;
    }

    internal void CheckOccupied(OccupancyStatus occupancyStatus) {
      IsOccupied = occupancyStatus.Occupancies[SectionId];
    }

    private void CheckActive() {
      IsActive = Turnouts.All(x => x.IsActive);
    }
  }
}
