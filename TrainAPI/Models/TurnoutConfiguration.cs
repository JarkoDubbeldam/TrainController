using Z21.Domain;

namespace TrainAPI.Models;

public class TurnoutConfiguration {
  public int Id { get; set; }
  public int TrackSectionId { get; set; }
  public TrackSection TrackSection { get; set; }
  public int TurnoutId { get; set; }
  public Turnout Turnout { get; set; }
  public TurnoutPosition TurnoutPosition { get; set; }  
}
