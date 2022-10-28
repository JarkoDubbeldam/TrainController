namespace TrainAPI.Trackers;

public interface ITracker<T> {
  T Get(int id);
  void Add(int id, T value);
  bool Remove(int id);
  ICollection<T> List();
}
