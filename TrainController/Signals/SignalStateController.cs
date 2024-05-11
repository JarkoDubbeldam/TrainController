using Microsoft.Extensions.Hosting;
using TrainController.Occupancies;
using TrainController.Turnouts;

namespace TrainController.Signals;
internal class SignalStateController(IController<Signal> signalController, IController<Turnout> turnoutController, IController<Occupancy> occupancyController) : BackgroundService {
  protected async override Task ExecuteAsync(CancellationToken stoppingToken) {
    while (!stoppingToken.IsCancellationRequested) {
      await ExecuteUpdate();
      await Task.Delay(1000, stoppingToken);
    }
  }

  private async Task ExecuteUpdate() {
    var turnouts = await turnoutController.List();
    var signals = await signalController.List();
    var occupancies = await occupancyController.List();

    foreach (var signal in signals.Values) {
      var colour = signal.SignalConfigurations.Select(x => DetermineColour(x, turnouts, occupancies, signals)).Min();
      var newSignal = signal with { SignalMode = signal.SignalMode ?? new SignalMode(colour, false, false, false) with { SignalColour = colour } };
      await signalController.Apply(newSignal);
    }
  }

  private static SignalColour DetermineColour(
    SignalConfiguration signalConfiguration,
    IReadOnlyDictionary<int, Turnout> turnouts,
    IReadOnlyDictionary<int, Occupancy> occupancies,
    IReadOnlyDictionary<int, Signal> signals) {
    if (!AreAllTurnoutsActivated(signalConfiguration.TurnoutConfigurations, turnouts)) {
      return SignalColour.Red;
    }
    if (!AreAllSectionsUnoccupied(signalConfiguration.GuardedSections, occupancies)) {
      return SignalColour.Red;
    }

    if (signalConfiguration.DownstringSignalId is not null &&
      signals.GetValueOrDefault(signalConfiguration.DownstringSignalId.Value)?.SignalStatus?.SignalColour == SignalColour.Red) {
      return SignalColour.Yellow;
    }

    return SignalColour.Green;
  }

  private static bool AreAllTurnoutsActivated(HashSet<TurnoutConfiguration> turnoutConfiguration, IReadOnlyDictionary<int, Turnout> turnouts) =>
    turnoutConfiguration.All(t => turnouts.GetValueOrDefault(t.TurnoutId)?.TurnoutStatus == t.TurnoutStatus);

  private static bool AreAllSectionsUnoccupied(HashSet<int> sections, IReadOnlyDictionary<int, Occupancy> occupancies) =>
    sections.All(s => occupancies.GetValueOrDefault(s)?.IsOccupied == true);
}
