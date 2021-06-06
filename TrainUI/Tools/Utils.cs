using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Avalonia;

namespace TrainUI.Tools {
  public static class Utils {
    public static double Distance(Point from, Point to) {
      var squaredDiffX = Math.Pow(from.X - to.X, 2);
      var squaredDiffY = Math.Pow(from.Y - to.Y, 2);
      return Math.Sqrt(squaredDiffY + squaredDiffX);
    }

    public static (double dx, double dy) GetEquation (Point a, Point b) {
      return (b.X - a.X, b.Y - a.Y);
    }

    public static IEnumerable<double> Range(int number) {
      return Enumerable.Range(0, number)
        .Select(x => (double)x / (number - 1));
    }

    public static IEnumerable<Point> GetInterpolations(Point a, Point b, int n) {
      var (dx, dy) = GetEquation(a, b);
      return Range(n)
        .Select(x => new Point(a.X + dx * x, a.Y + dy * x));
    }
  }
}
