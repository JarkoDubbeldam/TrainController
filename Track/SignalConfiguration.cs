﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using ReactiveUI;

using Z21.API;
using Z21.Domain;

namespace Track {
  public class SignalConfiguration : ReactiveObject {


    private SignalColour signalState;
    private readonly ILogger<SignalConfiguration> logger;

    public int Id { get; }
    public int SectionLength { get; }
    public SignalColour SignalState { get => signalState; private set {
        if (value != signalState) {
          logger.LogInformation("Setting signal {Id} state to {value}.", Id, value);
        }
        this.RaiseAndSetIfChanged(ref signalState, value);
      }
    }

    public SignalConfiguration(int id, int sectionLength, ILogger<SignalConfiguration> logger) {
      Id = id;
      SectionLength = sectionLength;
      this.logger = logger;
    }

    internal IDisposable SetupListener(TrackConnection parent) {
      Debug.Assert(parent.Signal == this);

      var d = new CompositeDisposable();

      var currents = parent.ToBoundary.Connections.Where(x => x.ViaSection.SectionId != parent.ViaSection.SectionId).ToList();
      var nextRound = new List<TrackConnection>();
      var currentDepth = 0;
      while (currentDepth++ <= SectionLength) {
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
          nextRound.AddRange(current.GetNextSections());          
        }

        currents = nextRound.Distinct().ToList();
        nextRound = new List<TrackConnection>();
      }

      return d;
    }


    public void HandleTurnoutsChanging(TurnoutChangingEventArgs args, TrackConnection parent) {
      if (args.Handled) {
        return;
      }

      if (!parent.ViaSection.IsActive) {
        return;
      }

      foreach(var nextSection in parent.WalkActiveSections().Take(SectionLength)) {
        if(nextSection.TrackConnectionState != TrackConnection.TrackConnectionIterator.TrackConnectionStateEnum.Active) {
          return;
        } else if (nextSection.TrackConnection.ViaSection.Turnouts.Any(x => x.TurnoutId == args.Address)) {
          args.DelayChange = TimeSpan.FromMilliseconds(1500);
          args.Handled = true;

          var newState = SignalColour.Red;
          SignalState = newState;
          return;
        } else if (nextSection.TrackConnection.Signal != null) {
          // Stop iterating if there is a signal on the section.
          return;
        }
      }
    }

    private void UpdateSignalState(TrackConnection parentTrackConnection) {
      Debug.Assert(parentTrackConnection.Signal == this);
      var newState = GetSignalState(parentTrackConnection);
      if (newState.HasValue) {
        SignalState = newState.Value;
      }
    }

    private SignalColour? GetSignalState(TrackConnection currentTrackConnection) {
      Debug.Assert(currentTrackConnection.Signal == this);
      if (!currentTrackConnection.ViaSection.IsActive) {
        // If current section is not active, another version of this section id has the lead over what happens with the signal.
        return null;
      }

      foreach (var nextSection in currentTrackConnection.WalkActiveSections().Take(SectionLength)) {
        // If the next section is inactive or missing, return red.
        if(nextSection.TrackConnectionState != TrackConnection.TrackConnectionIterator.TrackConnectionStateEnum.Active) {
          return SignalColour.Red;
        }

        // If there is a section, check if it is occupied
        if (nextSection.TrackConnection.ViaSection.IsOccupied) {
          return SignalColour.Red;
        }

        // If the next section is guarded by a signal, don't change the state directly, but instead change signal based on updates from linked signals.
        if(nextSection.TrackConnection.Signal != null) {
          return nextSection.TrackConnection.Signal.SignalState == SignalColour.Red ? SignalColour.Yellow : SignalColour.Green;
        }
      }

      return SignalColour.Green;
    }
  }
}
