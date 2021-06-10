using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockMap {
  public static class EnumerableExtensions {
    internal static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> items) => items.Select((item, index) => (item, index));

    /// <summary>Batch into size chunks lazily</summary>
    public static async IAsyncEnumerable<T[]> Batch<T>(this IAsyncEnumerable<T> items, int size) {
      var batch = new List<T>();
      await foreach (var item in items) {
        batch.Add(item);
        if (batch.Count < size) continue;
        yield return batch.ToArray();
        batch.Clear();
      }
      if (batch.Count > 0)
        yield return batch.ToArray();
    }

    /// <summary>If items is null return an empty set, if an item is null remove it from the list</summary>
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T>? items)
      => items?.Where(i => !i.NullOrDefault()) ?? Array.Empty<T>();

    public static IAsyncEnumerable<T> NotNull<T>(this IAsyncEnumerable<T> items) => items.Where(i => !i.NullOrDefault());
    
    public static async IAsyncEnumerable<T> SelectMany<T>(this IAsyncEnumerable<IEnumerable<T>> items) {
      await foreach (var g in items)
      foreach (var i in g)
        yield return i;
    }
  }
}