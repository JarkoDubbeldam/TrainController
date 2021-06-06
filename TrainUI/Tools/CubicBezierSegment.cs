using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Avalonia;
using Avalonia.Media;

namespace TrainUI.Tools {
  public class CubicBezierSegment : PathSegment, IBezierSegment {
    public static StyledProperty<Point> Control1Property = AvaloniaProperty.Register<CubicBezierSegment, Point>(nameof(Control1));
    public static StyledProperty<Point> Control2Property = AvaloniaProperty.Register<CubicBezierSegment, Point>(nameof(Control2));
    public static StyledProperty<Point> EndProperty = AvaloniaProperty.Register<CubicBezierSegment, Point>(nameof(End));

    public CubicBezierSegment() {
    }

    public Point Control1 { get => GetValue(Control1Property); set => SetValue(Control1Property, value); }
    public Point Control2 { get => GetValue(Control2Property); set => SetValue(Control2Property, value); }
    public Point  End { get => GetValue(EndProperty); set => SetValue(EndProperty, value); }

    protected override void ApplyTo(StreamGeometryContext ctx) {
      ctx.CubicBezierTo(Control1, Control2, End);
    }

    public IEnumerable<Point> GetPoints(Point from, int n) {
      var first = new Interpolator(from, Control1);
      var second = new Interpolator(Control1, Control2);
      var third = new Interpolator(Control2, End);

      foreach(var step in Utils.Range(n)) {
        var quad = new QuadraticBezierSegment { Control = second[step], End = third[step] };
        yield return quad.GetPoint(first[step], step);
      }
    }

    public double GetMinimalDistance(Point from, Point reference, int n) =>
      GetPoints(from, n).Min(x => Utils.Distance(x, reference));
  }
}
