namespace TrainController.Signals;

public record SignalConfiguration(HashSet<int> GuardedSections, HashSet<TurnoutConfiguration> TurnoutConfigurations, int? DownstringSignalId);
