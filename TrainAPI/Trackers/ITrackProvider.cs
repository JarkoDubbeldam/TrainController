using TrainAPI.Models;

namespace TrainAPI.Trackers;
public interface ITrackProvider {
  TrackSection GetById(int id);
  ICollection<TrackSection> GetBySectionId(int sectionId);
  TrackSection? GetNextActiveSection(int fromId);
  TrackSection? GetOppositeDirection(int id);
  Task Load();
}