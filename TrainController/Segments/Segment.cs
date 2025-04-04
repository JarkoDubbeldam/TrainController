namespace TrainController.Segments;
public record Segment(SegmentConfiguration Configuration, SegmentMode SegmentMode = SegmentMode.Disabled, SegmentMode ActualSegmentMode = SegmentMode.Disabled);
