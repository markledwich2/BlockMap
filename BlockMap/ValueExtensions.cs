using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockMap {
  static class ValueExtensions {
    public static bool In<T>(this T value, params T[] values) where T : IComparable => values.Contains(value);
    public static bool NullOrDefault<T>(this T value) => EqualityComparer<T>.Default.Equals(value, y: default);
  }
}