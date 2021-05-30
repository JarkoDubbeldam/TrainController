using System.Collections.Generic;

using Avalonia;

namespace TrainUI.Tools {
  public interface IBezierSegment {
    IEnumerable<Point> GetPoints(Point from, int n);
  }
}