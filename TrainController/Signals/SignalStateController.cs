using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using TrainController.Occupancies;
using TrainController.Turnouts;

namespace TrainController.Signals;
internal class SignalStateController(
  IController<Signal> signalController,
  IController<Turnout> turnoutController,
  IController<Occupancy> occupancyController,
  ILogger<SignalStateController> logger) : BackgroundService {
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
      logger.LogTrace("Checking for signal {signal}", signal.Id);
      var colour = signal.SignalConfigurations.Select(x => DetermineColour(x, turnouts, occupancies, signals)).Min();
      var newSignal = signal with { SignalMode = (signal.SignalMode ?? new SignalMode(colour, false, false, false)) with { SignalColour = colour } };

      if (signal != newSignal) {
        logger.LogInformation("Signal changed from {old} to {new}", signal, newSignal);
        logger.LogDebug("{signal} should be colour {colour}", signal.Id, colour);
        await signalController.Apply(newSignal);
      }
    }
  }

  private SignalColour DetermineColour(
    SignalConfiguration signalConfiguration,
    IReadOnlyDictionary<int, Turnout> turnouts,
    IReadOnlyDictionary<int, Occupancy> occupancies,
    IReadOnlyDictionary<int, Signal> signals) {
    foreach(var turnoutConfig in signalConfiguration.TurnoutConfigurations) {
      if(!turnouts.TryGetValue(turnoutConfig.TurnoutId, out var turnout)) {
        logger.LogTrace("TurnoutId {turnoudId} was not found.", turnoutConfig.TurnoutId);
        return SignalColour.Red;
      }
      if(turnout.TurnoutStatus != turnoutConfig.TurnoutMode) {
        logger.LogTrace("TurnoutId {turnoudId} was in mode {actual}, not {required}.", turnoutConfig.TurnoutId, turnout.TurnoutStatus, turnoutConfig.TurnoutMode);
        return SignalColour.Red;
      }
    }

    //if (!AreAllTurnoutsActivated(signalConfiguration.TurnoutConfigurations, turnouts)) {
    //  return SignalColour.Red;
    //}
    foreach(var section in signalConfiguration.GuardedSections) {
      if(!occupancies.TryGetValue(section, out var occupancy)) {
        logger.LogTrace("Section {section} was not found.", section);
        return SignalColour.Red;
      }
      if (occupancy.IsOccupied) {
        logger.LogTrace("Section {section} was occupied.", section);
        return SignalColour.Red;
      }
    }
    //if (!AreAllSectionsUnoccupied(signalConfiguration.GuardedSections, occupancies)) {
    //  return SignalColour.Red;
    //}

    if (signalConfiguration.DownstringSignalId is not null &&
      signals.GetValueOrDefault(signalConfiguration.DownstringSignalId.Value)?.SignalStatus?.SignalColour == SignalColour.Red) {
      logger.LogTrace("Next signal {signal} was red. Therefore yellow.", signalConfiguration.DownstringSignalId);
      return SignalColour.Yellow;
    }

    logger.LogTrace("All safe!");
    return SignalColour.Green;
  }

  private static bool AreAllTurnoutsActivated(HashSet<TurnoutConfiguration> turnoutConfiguration, IReadOnlyDictionary<int, Turnout> turnouts) =>
    turnoutConfiguration.All(t => turnouts.GetValueOrDefault(t.TurnoutId)?.TurnoutStatus == t.TurnoutMode);

  private static bool AreAllSectionsUnoccupied(HashSet<int> sections, IReadOnlyDictionary<int, Occupancy> occupancies) =>
    sections.All(s => occupancies.GetValueOrDefault(s)?.IsOccupied == true);
}
