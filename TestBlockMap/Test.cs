using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlockMap;
using FluentAssertions;
using Humanizer;
using NUnit.Framework;
// ReSharper disable PossibleMultipleEnumeration

namespace TestBlockMap {
  public class Tests {
    async IAsyncEnumerable<string> ReadFilesAsync(int count) {
      foreach (var f in Enumerable.Range(start: 1, count).Select(i => $"file{i}.json").ToArray()) yield return f;
    }

    [Test]
    public async Task BlockForBasics() {
      var toDownload = ReadFilesAsync(8);
      var downloaded = new ConcurrentBag<string>();

      await toDownload.BlockFor(async f => {
        await DownloadFile(f);
        Console.WriteLine($"downloaded file {f}");
        downloaded.Add(f); // for unit test, would use BlockMap if we actually wanted to have a collection returned.
      }, parallel: 2);
      downloaded.Should().BeEquivalentTo(await toDownload.ToArrayAsync());
    }

    [Test]
    public async Task BlockMapBasics() {
      const int numFiles = 20;
      const int combineSize = 4;
      var toDownload = ReadFilesAsync(numFiles);

      var uploaded = await toDownload
        .BlockMap(async f => {
          var local = await DownloadFile(f);
          Console.WriteLine($"downloaded file {f}");
          return local;
        }, parallel: 4)
        .Batch(combineSize)
        .BlockMap(async (b, i) => {
          await CombineFiles(b, i);
          Console.WriteLine($"combined files ({string.Join(", ", b)}) files to combined-{i}.ndjson");
          return b;
        }, parallel: 2)
        .SelectMany()
        .ToArrayAsync();

      uploaded.Length.Should().Be(numFiles);
    }
    
    [Test]
    public async Task BlockShouldThrow() {
      string[] downloaded = null;
      try {
        downloaded = await ReadFilesAsync(10)
          .BlockMap(async (f, i) => {
            if (i == 5) throw new ("unhandled error in map");
            await Task.Delay(500.Milliseconds());
            Console.WriteLine($"downloading file {f}");
            return f;
          }, parallel: 4)
          .ToArrayAsync();
      }
      catch (Exception ex) {
        ex.Message.Should().Be("unhandled error in map");
      }
      downloaded.Should().BeNull();
    }

    static async Task CombineFiles(string[] b, int i) => await Task.Delay(1.Seconds());

    static async Task<string> DownloadFile(string f) {
      await Task.Delay(500.Milliseconds());
      return $"local/{f}";
    }
  }
}