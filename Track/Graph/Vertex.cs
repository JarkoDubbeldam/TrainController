using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Track.Graph {
  public class Vertex<T> {
    public Vertex(T value) {
      Value = value;
    }


    public T Value { get; }
    public ICollection<WeightedDirectedEdge<T>> Neighbors { get; } = new HashSet<WeightedDirectedEdge<T>>();
  }
}
