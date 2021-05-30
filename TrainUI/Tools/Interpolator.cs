using System;
using System.Collections.Generic;
using System.Text;

using Avalonia;

namespace TrainUI.Tools {
  public class Interpolator {
    private readonly Point a;
    private readonly Point b;
    private readonly (double dx, double dy) deltas;

    public Interpolator(Point a, Point b) {
      this.a = a;
      this.b = b;
      deltas = Utils.GetEquation(a, b);
    }

    public Point this[double step] {
      get {
        if(step > 1 || step < 0) {
          throw new ArgumentOutOfRangeException(nameof(step));
        }
        return new Point(a.X + deltas.dx * step, a.Y + deltas.dy * step);
      }
    }
  }
}
