using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;

namespace TrainUI.Tools {
  public class QuadraticBezierSegment : IBezierSegment {
    public Point End { get; set; }
    public Point Control { get; set; }

    public IEnumerable<Point> GetPoints(Point from, int n) {
      var first = new Interpolator(from, Control);
      var second = new Interpolator(Control, End);
      return Utils.Range(n)
        .Select(step => new Interpolator(first[step], second[step])[step]);
    }

    public Point GetPoint(Point from, double step) {
      if (step > 1 || step < 0) {
        throw new ArgumentOutOfRangeException(nameof(step));
      }
      var first = new Interpolator(from, Control);
      var second = new Interpolator(Control, End);
      return new Interpolator(first[step], second[step])[step];
    }
  }
}
