namespace TrainController.Turnout;
public class Turnout {
  public int Id { get; set; }
  /// <summary>
  /// Turnout status as specified by the user
  /// </summary>
  public TurnoutStatus TurnoutMode { get; set; }
  /// <summary>
  /// Turnout status as reported by the track.
  /// </summary>
  public TurnoutStatus TurnoutStatus { get; set; }
  public long Timestamp { get; set; }
}

public enum TurnoutStatus {
  Unspecified,
  Left,
  Right
}
