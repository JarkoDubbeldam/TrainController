using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

using Newtonsoft.Json.Linq;

using ReactiveUI;

using Z21;
using Z21.API;
using Z21.Domain;

namespace Track {
  public class TrackRepository : ITrackRepository {
    private readonly Dictionary<int, TrackSection> trackSections;
    private readonly Dictionary<int, TrackSectionBoundary> boundaries;
    private readonly ILookup<int, TurnoutConfiguration> turnouts;

    private TrackRepository(Dictionary<int, TrackSection> trackSections, Dictionary<int, TrackSectionBoundary> boundaries) {
      this.trackSections = trackSections;
      this.boundaries = boundaries;
      this.turnouts = Sections.SelectMany(x => x.Turnouts).Distinct().ToLookup(x => x.TurnoutId);
    }

    public IReadOnlyCollection<TrackSection> Sections => trackSections.Values;
    public IReadOnlyCollection<TrackSectionBoundary> Boundaries => boundaries.Values;

    public IDisposable SetupSubscriptions(IZ21Client client) {
      var disposer = new CompositeDisposable();
      client.TurnoutInformationChanged
        .Subscribe(x => {
          var turnouts = this.turnouts[x.Address];
          foreach (var turnout in turnouts) {
            turnout.CheckIfActive(x);
          }
        })
        .DisposeWith(disposer);

      client.OccupancyStatusChanged
        .Subscribe(x => {
          foreach (var section in Sections) {
            section.CheckOccupied(x);
          }
        })
        .DisposeWith(disposer);

      foreach (var section in Sections) {
        section.SetupListener().DisposeWith(disposer);
      }

      var connectionsWithSignal = Boundaries.SelectMany(x => x.Connections).Distinct().Where(x => x.Signal != null);
      foreach( var connectionWithSignal in connectionsWithSignal) {
        var signal = connectionWithSignal.Signal;
        signal.SetupListener(connectionWithSignal).DisposeWith(disposer);
        client.TurnoutChanging.Subscribe(x => signal.HandleTurnoutsChanging(x, connectionWithSignal)).DisposeWith(disposer);

        connectionWithSignal.Signal.WhenAnyValue(x => x.SignalState)
                    .Throttle(TimeSpan.FromMilliseconds(500))
          .DistinctUntilChanged()
          .Subscribe(colour => client.SetSignal(new SetSignalRequest {
            Address = (short)signal.Id,
            SignalMode = new SignalMode {
              SignalColour = signal.SignalState,
              Blinking = false,
              Number = false,
              NightMode = false
            }
          }))
          .DisposeWith(disposer);
      }

      return disposer;
    }

    public static TrackRepository FromJson(string json) {
      var halfParsed = JObject.Parse(json);
      var boundariesDict = halfParsed["Boundaries"].ToDictionary(x => x["Id"], y => new TrackSectionBoundary {
        Id = y["Id"].ToObject<int>()
      });

      foreach (var item in halfParsed["Boundaries"]) {
        var boundary = boundariesDict[item["Id"]];
        boundary.Connections = item["Connections"]
          .Select(x => DeserializeTrackConnection(x, boundariesDict))
          .ToList();
      }
      var sections = boundariesDict.Values
        .SelectMany(x => x.Connections.Select(y => y.ViaSection))
        .GroupBy(x => x.Id, (key, element) => element.First())
        .ToDictionary(x => x.Id);
      foreach (var boundary in boundariesDict.Values) {
        foreach (var connection in boundary.Connections) {
          connection.ViaSection = sections[connection.ViaSection.Id];
          connection.ViaSection.AddTrackSectionBoundary(boundary);
        }
      }

      return new TrackRepository(sections, boundariesDict.ToDictionary(x => x.Key.ToObject<int>(), x => x.Value));
    }

    private static TrackConnection DeserializeTrackConnection(JToken token, Dictionary<JToken, TrackSectionBoundary> boundariesDict) {
      return new TrackConnection {
        ViaSection = DeserializeTrackSection(token["ViaSection"]),
        ToBoundary = boundariesDict[token["ToBoundaryId"]],
        Signal = token["Signal"]?.ToObject<SignalConfiguration>()
      };
    }

    private static TrackSection DeserializeTrackSection(JToken token) {
      var result = new TrackSection {
        Id = token["Id"].ToObject<int>(),
        SectionId = token["SectionId"].ToObject<int>(),
        Turnouts = token["Turnouts"].Select(DeserializeTurnoutConfiguration).ToList()
      };
      return result;
    }

    private static TurnoutConfiguration DeserializeTurnoutConfiguration(JToken token) {
      return new TurnoutConfiguration { TurnoutId = token["TurnoutId"].ToObject<int>(), TurnoutMode = token["TurnoutMode"].ToObject<TurnoutMode>() };
    }
  }
}
