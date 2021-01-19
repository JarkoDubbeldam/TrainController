using System;
using System.Linq;

using Track.Graph;

namespace TrackTester {
  class Program {
    static void Main(string[] args) {
      var graph = new DirectedGraph<int>();
      foreach(var node in Enumerable.Range(1, 7)) {
        graph.AddNodes(node, -node);
      }
      graph.AddDirectedEdge(1, 6);
      graph.AddDirectedEdge(6, 5);
      graph.AddDirectedEdge(6, 7);
      graph.AddDirectedEdge(5, 4);
      graph.AddDirectedEdge(4, 3);
      graph.AddDirectedEdge(3, -7);
      graph.AddDirectedEdge(3, 2);
      graph.AddDirectedEdge(2, 1);
      graph.AddDirectedEdge(-7, -6);
      graph.AddDirectedEdge(-6, -1);
      graph.AddDirectedEdge(-1, -2);
      graph.AddDirectedEdge(-2, -3);
      graph.AddDirectedEdge(-3, -4);
      graph.AddDirectedEdge(-4, -5);
      graph.AddDirectedEdge(-5, -6);
      graph.AddDirectedEdge(7, -3);

      var route = graph.FindRoute(5, -5);
      Console.WriteLine(string.Join(" ", route));
    }
  }
}
