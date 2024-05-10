using System.Collections.Concurrent;
using System.Reactive.Subjects;
using Microsoft.Extensions.Hosting;
using Trains.DataAccess.Services;
using Z21;
using Z21.API;

namespace TrainController.Signals;
internal class SignalController(ISignalService signalService, IZ21Client z21Client) : BackgroundService, IController<Signal> {
  private readonly Subject<Signal> signalSubject = new();
  private readonly ConcurrentDictionary<int, Signal> signals = new();
  private long lastUpdate = DateTime.UtcNow.Ticks;
  private bool fetchedDatabaseYet = false;

  public IObservable<Signal> Observable => signalSubject;

  public async Task Apply(Signal value) {
    var dbSignal = new Trains.DataAccess.Models.Signal { Id = value.Id };
    await signalService.SaveSignal(dbSignal);

    var signal = signals.GetOrAdd(value.Id, new Signal { Id = dbSignal.Id, Timestamp = lastUpdate });
    if (signal.SignalMode != value.SignalMode) {
      signal.SignalMode = value.SignalMode;
      signal.Timestamp = lastUpdate;
    }
  }

  public async Task<Signal?> Get(int id) {
    if (!fetchedDatabaseYet) {
      await FetchDatabase();
    }
    return signals.GetValueOrDefault(id);
  }

  public async Task<List<Signal>> List() {
    if (!fetchedDatabaseYet) {
      await FetchDatabase();
    }

    return signals.Values.ToList();
  }

  private async Task FetchDatabase() {
    var databaseTurnouts = await signalService.ListSignals();
    foreach (var dbSignal in databaseTurnouts) {
      signals.TryAdd(dbSignal.Id, new Signal { Id = dbSignal.Id, Timestamp = lastUpdate });
    }
    fetchedDatabaseYet = true;
  }

  protected async override Task ExecuteAsync(CancellationToken stoppingToken) {
    while (!stoppingToken.IsCancellationRequested) {
      RunUpdateLoop();
      lastUpdate = DateTime.UtcNow.Ticks;
      await Task.Delay(500, stoppingToken);
    }
  }

  private void RunUpdateLoop() {
    foreach (var signal in signals.Values) {
      if (signal.Timestamp < lastUpdate) {
        continue;
      }

      if (signal.SignalMode == signal.SignalStatus || signal.SignalMode is null) {
        continue;
      }

      var intendedMode = signal.SignalMode;
      // Don't immediately change from red to green or the other way around. Instead move to yellow first.
      if ((signal.SignalMode.SignalColour == SignalColour.Red && signal.SignalStatus?.SignalColour == SignalColour.Green) ||
        (signal.SignalMode.SignalColour == SignalColour.Green && signal.SignalStatus?.SignalColour == SignalColour.Red)) {
        intendedMode = intendedMode with { SignalColour = SignalColour.Yellow };
        // Also set timestamp to be in the future in order to ensure this will be visited again.
        signal.Timestamp = long.MaxValue;
      } else {
        // reset timestamp to prevent continuously updating.
        signal.Timestamp = lastUpdate;
      }

      z21Client.SetSignal(new SetSignalRequest {
        Address = (short)signal.Id,
        SignalMode = Map(intendedMode)
      });
    }
  }

  private static Z21.Domain.SignalMode Map(SignalMode intendedMode) => new() {
    SignalColour = Map(intendedMode.SignalColour),
    Blinking = intendedMode.Blinking,
    Number = intendedMode.ShowNumber,
    NightMode = intendedMode.NightMode
  };

  private static Z21.Domain.SignalColour Map(SignalColour signalColour) => signalColour switch {
    SignalColour.Green => Z21.Domain.SignalColour.Green,
    SignalColour.Yellow => Z21.Domain.SignalColour.Yellow,
    SignalColour.Red => Z21.Domain.SignalColour.Red,
    _ => throw new ArgumentOutOfRangeException(nameof(signalColour))
  };
}
