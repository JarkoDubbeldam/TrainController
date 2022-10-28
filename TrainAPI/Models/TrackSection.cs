namespace TrainAPI.Models;

public class TrackSection {
  public int Id { get; set; }
  public int ForeignId { get; set; }
  public int SectionId { get; set; }
  public int FromBoundaryId { get; set; }

  
  public TrackBoundary FromBoundary { get; set; }
  public int ToBoundaryId { get; set; }
  public TrackBoundary ToBoundary { get; set; }

  public ICollection<TurnoutConfiguration> TurnoutConfigurations { get; set; }
}
