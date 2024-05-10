using System.Linq;

namespace Z21;
internal static class LinqExtensions {
  public static bool MatchesPattern(this byte[] bytes, byte?[] pattern) => bytes.Zip(pattern, (r, p) => p == null || p == r).All(x => x);
}
