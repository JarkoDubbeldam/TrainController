
using System.Collections.Concurrent;
using System.Reactive.Subjects;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Trains.DataAccess.Services;

using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainController.Turnouts;
internal class TurnoutController(ITurnoutService turnoutService, IZ21Client z21Client, ILogger<TurnoutController> logger) : BackgroundService, IController<Turnout> {
  private readonly Subject<Turnout> turnoutSubject = new();
  private readonly ConcurrentDictionary<int, Turnout> turnouts = new();
  private long lastUpdate = DateTime.UtcNow.Ticks;
  private bool fetchedDatabaseYet = false;

  public IObservable<Turnout> Observable => turnoutSubject;

  public async Task Apply(Turnout value) {
    var newTurnout = new Turnout { Id = value.Id, Timestamp = lastUpdate };
    var turnout = turnouts.AddOrUpdate(value.Id, newTurnout, (_, existing) => {
      if (existing.TurnoutMode != value.TurnoutMode) {
        return existing with { TurnoutMode = value.TurnoutMode, Timestamp = lastUpdate };
      }
      return existing;
    });

    if (object.ReferenceEquals(turnout, newTurnout)) {
      var dbTurnout = new Trains.DataAccess.Models.Turnout { Id = value.Id };
      await turnoutService.SaveTurnout(dbTurnout);
    }

  }

  public async Task<Turnout?> Get(int id) {
    if (!fetchedDatabaseYet) {
      await FetchDatabase();
    }
    return turnouts.GetValueOrDefault(id);
  }

  public async Task<IReadOnlyDictionary<int, Turnout>> List() {
    if (!fetchedDatabaseYet) {
      await FetchDatabase();
    }

    return turnouts;
  }

  private async Task FetchDatabase() {
    var databaseTurnouts = await turnoutService.ListTurnouts();
    foreach (var dbTurnout in databaseTurnouts) {
      turnouts.TryAdd(dbTurnout.Id, new Turnout { Id = dbTurnout.Id, Timestamp = lastUpdate });
    }
    fetchedDatabaseYet = true;
  }

  protected async override Task ExecuteAsync(CancellationToken stoppingToken) {
    using var listeningSubscription = z21Client.TurnoutInformationChanged
      .Subscribe(OnTurnoutChanged);

    while (!stoppingToken.IsCancellationRequested) {
      RunUpdateLoop();
      lastUpdate = DateTime.UtcNow.Ticks;
      await Task.Delay(500, stoppingToken);
    }
  }

  private void RunUpdateLoop() {
    foreach (var turnout in turnouts.Values) {
      //if (turnout.Timestamp < lastUpdate) {
      //  continue;
      //}

      var mutableTurnout = turnout;

      if (turnout.TurnoutMode == TurnoutStatus.Unspecified) {
        mutableTurnout = mutableTurnout with { TurnoutStatus = TurnoutStatus.Unspecified };
      }

      // If a change was requested through Apply:
      if (turnout.TurnoutMode != TurnoutStatus.Unspecified && turnout.TurnoutMode != turnout.TurnoutStatus) {
        try {
          z21Client.SetTurnout(new SetTurnoutRequest {
            Address = (short)turnout.Id,
            TurnoutPosition = Map(turnout.TurnoutMode)
          });
          mutableTurnout = mutableTurnout with { TurnoutStatus = turnout.TurnoutMode };
        } catch (TimeoutException) {
          logger.LogInformation("Waiting for response about setting turnout timed out.");
        }
      }

      if (mutableTurnout != turnout) {
        logger.LogInformation("Turnout {old} changed to {new}", turnout, mutableTurnout);
        turnouts.AddOrUpdate(turnout.Id, mutableTurnout, (_, _) => mutableTurnout);
        turnoutSubject.OnNext(turnout);
      }
    }
  }


  private void OnTurnoutChanged(TurnoutInformation information) {
    var turnout = turnouts.AddOrUpdate(information.Address, new Turnout {
      Id = information.Address,
      TurnoutStatus = Map(information.TurnoutPosition),
      Timestamp = lastUpdate,
      TurnoutMode = TurnoutStatus.Unspecified
    }, (_, existing) => {
      var newStatus = Map(information.TurnoutPosition);
      return existing with { TurnoutStatus = newStatus, Timestamp = lastUpdate, TurnoutMode = existing.TurnoutMode != newStatus ?  TurnoutStatus.Unspecified : existing.TurnoutMode };
    });

    turnoutSubject.OnNext(turnout);
  }

  private static TurnoutPosition Map(TurnoutStatus turnoutMode) => turnoutMode switch {
    TurnoutStatus.Left => TurnoutPosition.Position2,
    TurnoutStatus.Right => TurnoutPosition.Position1,
    _ => throw new ArgumentOutOfRangeException(nameof(turnoutMode))
  };

  private static TurnoutStatus Map(TurnoutPosition turnoutPosition) => turnoutPosition switch {
    TurnoutPosition.Position2 => TurnoutStatus.Left,
    TurnoutPosition.Position1 => TurnoutStatus.Right,
    _ => TurnoutStatus.Unspecified
  };
}
