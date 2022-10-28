namespace TrainAPI.Models;

public class TrackBoundary {
  public int Id { get; set; }
  public ICollection<TrackSection> ToTrackSections { get; set; }
  public ICollection<TrackSection> FromTrackSections { get; set; }
}
