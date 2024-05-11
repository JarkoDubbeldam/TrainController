
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using Microsoft.Extensions.Hosting;
using Trains.DataAccess.Services;
using Z21;
using Z21.API;
using Z21.Domain;

namespace TrainController.Turnouts;
internal class TurnoutController(ITurnoutService turnoutService, IZ21Client z21Client) : BackgroundService, IController<Turnout> {
  private readonly Subject<Turnout> turnoutSubject = new();
  private readonly ConcurrentDictionary<int, Turnout> turnouts = new();
  private long lastUpdate = DateTime.UtcNow.Ticks;
  private bool fetchedDatabaseYet = false;

  public IObservable<Turnout> Observable => turnoutSubject;

  public async Task Apply(Turnout value) {
    var dbTurnout = new Trains.DataAccess.Models.Turnout { Id = value.Id };
    await turnoutService.SaveTurnout(dbTurnout);

    var turnout = turnouts.GetOrAdd(value.Id, new Turnout { Id = dbTurnout.Id, Timestamp = lastUpdate });
    if (turnout.TurnoutMode != value.TurnoutMode) {
      turnout.TurnoutMode = value.TurnoutMode;
      turnout.Timestamp = lastUpdate;
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
      await RunUpdateLoop();
      lastUpdate = DateTime.UtcNow.Ticks;
      await Task.Delay(500, stoppingToken);
    }
  }

  private async Task RunUpdateLoop() {
    foreach (var turnout in turnouts.Values) {
      if (turnout.Timestamp < lastUpdate) {
        continue;
      }

      // If a change was requested through Apply:
      if (turnout.TurnoutMode != TurnoutStatus.Unspecified && turnout.TurnoutMode != turnout.TurnoutStatus) {
        await z21Client.SetTurnout(new SetTurnoutRequest {
          Address = (short)turnout.Id,
          TurnoutPosition = Map(turnout.TurnoutMode)
        });
      }

      turnoutSubject.OnNext(turnout);
    }
  }


  private void OnTurnoutChanged(TurnoutInformation information) {
    var turnout = turnouts.GetOrAdd(information.Address, new Turnout {
      Id = information.Address
    });
    turnout.TurnoutStatus = Map(information.TurnoutPosition);
    // if the mode was set from z21
    if (turnout.TurnoutStatus != turnout.TurnoutMode) {
      turnout.TurnoutMode = TurnoutStatus.Unspecified;
    }
    turnout.Timestamp = lastUpdate;
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
