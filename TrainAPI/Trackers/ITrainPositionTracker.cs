namespace TrainAPI.Trackers;

public interface ITrainPositionTracker {
  TrainPosition GetTrainPosition(int trainId);
}