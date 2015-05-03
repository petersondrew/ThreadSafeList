# `ReaderWriterLockList<T>`
Provides a thread-safe implementation of `IList<T>` that takes a reader-priorized approach to synchronization and ensures thread-safe enumeration.

## Use cases
The implementation is optimized for low lock contention amongst readers and writers, as well as minimum lock contention between multiple readers.  
This is not an ideal class for a producer/consumer scenario. `BlockingCollection<T>`, one of the concurrent implementations of `IProducerConsumer<T>`, or TPL Dataflow would be better choices.

## Thread-safety notes
The thread-safety provided by this implementation is not a substitution for proper thread synchronization, any threads performing both reading and writing of values must ensure the list is locked if the outcome of a read can determine a write. For example:

```csharp
if (list[0] == "foo")
{
    // Another thread may Remove list[0] at this point, meaning we're about to modify a different list item
    // External locks should be acquired to synchronize reading and writing in scenarios such as this
    list[0] = "bar";
}
```

Users should also take care to synchronize threads that call `IList<T>.Remove` or `IList<T>.RemoveAt` as `IList<T>` does not behave like concurrent collections that allow for things like `TryRemove`. Implementing a `TryRemove` function is a possibility if `IList<T>.Remove` is simply a NO-OP, but that would be quite confusing to users of this class.

## Implementation
The `ReaderWriterLockList<T>` class provides thread-safe access to an underlying `List<T>` by utilizing the `ReaderWriterLockSlim` class.  

## Comparison to other solutions
Compared to a simple implementation that only uses `Monitor` locks, `ReaderWriterLockList<T>` provides much less contention among multiple readers, but can be slightly slower when adding items.  

## Tests
Tests for proper thread-safety when modifying the list have been provided, along with some basic performance tests emulating both multiple concurrent writing threads as well as multiple concurrent reading threads.
