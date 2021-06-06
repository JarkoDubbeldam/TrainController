using System;
using System.Collections.Generic;

namespace Track {
  internal static class LinqExtensions {
    public static IEnumerable<TOut> WhereSelect<TIn, TOut>(this IEnumerable<TIn> sequence, Func<TIn, (bool, TOut)> predicateSelector) {
      if (sequence is null) {
        throw new ArgumentNullException(nameof(sequence));
      }

      if (predicateSelector is null) {
        throw new ArgumentNullException(nameof(predicateSelector));
      }

      return WhereSelectInternal(sequence, predicateSelector);
    }

    private static IEnumerable<TOut> WhereSelectInternal<TIn, TOut>(IEnumerable<TIn> sequence, Func<TIn, (bool, TOut)> predicateSelector) {
      foreach (var item in sequence) {
        var result = predicateSelector(item);
        if (result.Item1) {
          yield return result.Item2;
        }
      }
    }

    public static T1 MinBy<T1, T2>(this IEnumerable<T1> sequence, Func<T1, T2> selector) {
      if (sequence is null) {
        throw new ArgumentNullException(nameof(sequence));
      }

      if (selector is null) {
        throw new ArgumentNullException(nameof(selector));
      }

      return MinByInternal(sequence, selector, Comparer<T2>.Default);
    }

    private static T1 MinByInternal<T1, T2>(IEnumerable<T1> sequence, Func<T1, T2> selector, IComparer<T2> comparer) {
      using var enumerator = sequence.GetEnumerator();
      if (!enumerator.MoveNext()) {
        return default;
      }

      T1 argmin = enumerator.Current;
      T2 min = selector(enumerator.Current);

      while (enumerator.MoveNext()) {
        var value = selector(enumerator.Current);
        if (comparer.Compare(min, value) > 1) {
          argmin = enumerator.Current;
          min = value;
        }
      }
      return argmin;
    }
  }
}
