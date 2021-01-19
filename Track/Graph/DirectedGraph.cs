using System;
using System.Collections.Generic;
using System.Linq;

namespace Track.Graph {
  public class DirectedGraph<T> {
    private readonly Dictionary<T, Vertex<T>> vertices;

    public DirectedGraph() {
      vertices = new Dictionary<T, Vertex<T>>();
    }
    public DirectedGraph(IEnumerable<Vertex<T>> vertices) {
      this.vertices = vertices.ToDictionary(x => x.Value, x => x);
    }
    public int Count => vertices.Count;
    public IReadOnlyDictionary<T, Vertex<T>> Vertices => vertices;

    public void AddNode(T value) => AddNode(new Vertex<T>(value));

    public void AddNode(Vertex<T> vertex) => vertices.Add(vertex.Value, vertex);
    public void AddNodes(params T[] values) {
      foreach (var value in values) {
        AddNode(value);
      }
    }
    public void AddDirectedEdge(T from, T to, int weight = 1) {
      vertices[from].Neighbors.Add(new WeightedDirectedEdge<T>(vertices[to], weight));
    }

    public IEnumerable<T> FindRoute(T from, T to) {
      if (from.Equals(to)) {
        return Array.Empty<T>();
      }
      var distances = vertices.ToDictionary(x => x.Value, x => x.Key.Equals(from) ? 0 : int.MaxValue);
      var routes = vertices.ToDictionary(x => x.Key, x => x.Key);
      var q = new HashSet<Vertex<T>>(vertices.Values);

      while (q.Count > 0) {
        KeyValuePair<Vertex<T>, int> n = default;
        // Get and pop from Q the lowest distance node.
        foreach (var value in distances.OrderBy(x => x.Value)) {
          if (q.Remove(value.Key)) {
            n = value;
            break;
          }
        }

        // Update neigbors if better distances
        foreach (var neighbor in n.Key.Neighbors) {
          if (distances[neighbor.Connection] > n.Value + neighbor.Weight) {
            distances[neighbor.Connection] = n.Value + neighbor.Weight;
            routes[neighbor.Connection.Value] = n.Key.Value;
          }
        }

      }

      return EnumerateRouteReversed(from, to, routes).Reverse();

    }

    private IEnumerable<T> EnumerateRouteReversed(T from, T to, Dictionary<T, T> connections) {
      T currentValue = to;
      yield return currentValue;
      while (!currentValue.Equals(from)) {
        currentValue = connections[currentValue];
        yield return currentValue;
      }
    }
  }
}