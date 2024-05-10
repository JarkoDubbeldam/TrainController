namespace TrainController.Signals;
public record Signal(int Id, List<SignalConfiguration> SignalConfigurations, long Timestamp, SignalMode? SignalMode = null, SignalMode? SignalStatus = null);
