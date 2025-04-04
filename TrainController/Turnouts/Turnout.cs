namespace TrainController.Turnouts;
public record Turnout {
  public int Id { get; init; }
  /// <summary>
  /// Turnout status as specified by the user
  /// </summary>
  public TurnoutStatus TurnoutMode { get; init; }
  /// <summary>
  /// Turnout status as reported by the track.
  /// </summary>
  public TurnoutStatus TurnoutStatus { get; init; }
  public long Timestamp { get; init; }
}

public enum TurnoutStatus {
  Unspecified,
  Left,
  Right
}
