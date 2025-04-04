namespace TrainController.Segments;
public record SegmentConfiguration(int Id, string Name, List<TurnoutConfiguration> Turnouts, List<int> MutuallyExclusiveSegments);
