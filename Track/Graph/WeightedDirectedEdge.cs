using System;
using System.Collections.Generic;
using System.Text;

namespace Track.Graph {
  public class WeightedDirectedEdge<T> {
    public WeightedDirectedEdge(Vertex<T> connection, int weight) {
      Connection = connection;
      Weight = weight;
    }

    public int Weight { get; set; }
    public Vertex<T> Connection { get; set; }
  }
}
