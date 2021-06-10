using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockMap {
  static class TaskExtensions {
    public static Task Delay(this TimeSpan timespan, CancellationToken cancel = default) => Task.Delay(timespan, cancel);
  }
}