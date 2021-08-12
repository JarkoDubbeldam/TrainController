using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

using ReactiveUI;

using Z21.Domain;

namespace Track {
  public class SignalConfiguration : ReactiveObject {
    const int MAX_SECTION_LENGTH = 6;

    private SignalColour signalState;
    public int Id { get; }
    public SignalColour SignalState { get => signalState; private set => this.RaiseAndSetIfChanged(ref signalState, value); }

    public SignalConfiguration(int id) => Id = id;

    internal IDisposable SetupListener(TrackConnection parent) {
      var d = new CompositeDisposable();

      var currents = parent.ToBoundary.Connections.Where(x => x.ViaSection.SectionId != parent.ViaSection.SectionId).ToList();
      var nextRound = new List<TrackConnection>();
      var currentDepth = 0;
      while (currentDepth++ <= MAX_SECTION_LENGTH) {
        foreach(var current in currents) {
          // Subscribe to section's occupancy and activeness fields.
          current.ViaSection.WhenAnyValue(x => x.IsOccupied, x => x.IsActive)
            .DistinctUntilChanged()
            .Subscribe(x => UpdateSignalState(parent))
            .DisposeWith(d);

          // If section has a signal, we don't need to subscribe any further. We do want to subscribe to the signal's state though.
          if(current.Signal != null) {
            current.Signal.WhenAnyValue(x => x.SignalState)
              .DistinctUntilChanged()
              .Subscribe(x => UpdateSignalState(parent))
              .DisposeWith(d);
            continue;
          }

          // Otherwise, add all new sections to the list.
          nextRound.AddRange(current.ToBoundary.Connections.Where(x => x.ViaSection.SectionId != current.ViaSection.SectionId));          
        }

        currents = nextRound.Distinct().ToList();
        nextRound = new List<TrackConnection>();
      }

      return d;
    }

    private void UpdateSignalState(TrackConnection parentTrackConnection) {
      Debug.Assert(parentTrackConnection.Signal == this);
      var newState = GetSignalState(parentTrackConnection);
      Debug.WriteLine($"Setting signal state to {newState}.");
      SignalState = newState;
    }

    private SignalColour GetSignalState(TrackConnection currentTrackConnection) {
      var currentDepth = 0;
      SignalConfiguration nextSignal = null;

      while (currentDepth++ <= MAX_SECTION_LENGTH) {
        // Get next connection that's not the same sectionId as current (no backtracking)
        // There should be at most a single active section left over. If there are none, the section is unsafe due
        // to incompatible turnout placement.
        var nextConnection = currentTrackConnection.ToBoundary.Connections
          .Where(x => x.ViaSection.SectionId != currentTrackConnection.ViaSection.SectionId)
          .FirstOrDefault(x => x.ViaSection.IsActive);
        if (nextConnection == null) {
          return SignalColour.Red;
        }

        // If there is a section, check if it is occupied
        if (nextConnection.ViaSection.IsOccupied) {
          return SignalColour.Red;
        }

        // If there is a signal guarding the section, we can stop looking.
        if (nextConnection.Signal != null) {
          nextSignal = nextConnection.Signal;
          break;
        }

        currentTrackConnection = nextConnection;
      }

      // If there is no signal, all is clear, else we maybe return yellow if next signal is red.
      if (nextSignal?.SignalState == SignalColour.Red) {
        return SignalColour.Yellow;
      } else {
        return SignalColour.Green;
      }
    }
  }
}
