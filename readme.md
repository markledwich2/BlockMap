# BlockMap
Fluent Async parallelism using the TPL Dataflow library.

This example uses **BlockMap** and **BlockFor** to: 
- download an async enumerable of json file urls (4 concurrent downloads)
- batch the contents into lines in an ndjson files (2 concurrent file writes)

```c#
var uploaded = await ReadFilesAsync() // start with IEnumerable/IAsyncEnumerable
    .BlockMap(async f => { // BlockMap takes a task return a result and run's it with the configured parallelism
        var local = await DownloadFile(f);
        Console.WriteLine($"downloaded file {f}");
        return local; // the return values are available as an IAsynEnumerable
    }, parallel: 4)
    .Batch(4) // continue with IAsyncEnumerable (Highly recommend System.Linq.Async)
    .BlockFor(async (b, i) => { // BlockFor takes a task with no result and executes with the configured parallelism
        await CombineFiles(b, i);
        Console.WriteLine($"combined files ({string.Join(", ", b)}) files to combined-{i}.ndjson");
    }, parallel: 2)
```


## Pitch
**System.Threading.Tasks.Dataflow** is an underrated part of the .net library. I think it is underused because the learning curve is steep and has too much ceremony.

BlockMap is a simple function on IAsyncEnumerable that will let you take advantage of producer/consumer async parallelism without thinking much about it. Use this when you have IO bound tasks that benefit from parallelism.

Main benefits
- Safe parallelism. Don't modify anything outside the scope of your task, return new sate and your safe.
- Combine naturally with IASyncEnumerable and the System.Linq.Async library, or your own IASynEnumerable extensions
- Lazily evaluate results that are too big to fit in memory. Slow blocks will back 
- BlockMap has a configurable buffer, so that a source enumerable will be slowed down if the buffer is full



## Limitations
- Requires .net > x??
- Only supports the most common use case for TPL Dataflow. There are patterns/blocks that are not surfaced yet (e.g. Broadcast).

