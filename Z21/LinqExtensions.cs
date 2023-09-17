using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z21;
internal static class LinqExtensions {
  public static bool MatchesPattern(this byte[] bytes, byte?[] pattern) => bytes.Zip(pattern, (r, p) => p == null || p == r).All(x => x);
}
