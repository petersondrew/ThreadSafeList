using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xbehave;

namespace List
{
    public sealed class ThreadSafeListSpecs
    {
        [Scenario]
        [Example(typeof (SimpleLockList<int>), 1, 1000, 1000)]
        [Example(typeof (SimpleLockList<int>), 2, 1000, 2000)]
        [Example(typeof (SimpleLockList<int>), 4, 1000, 4000)]
        [Example(typeof (SimpleLockList<int>), 8, 1000, 8000)]
        [Example(typeof (ReaderWriterLockList<int>), 1, 1000, 1000)]
        [Example(typeof (ReaderWriterLockList<int>), 2, 1000, 2000)]
        [Example(typeof (ReaderWriterLockList<int>), 4, 1000, 4000)]
        [Example(typeof (ReaderWriterLockList<int>), 8, 1000, 8000)]
        public void Add(Type listType, int threads, int items, int expectedCount, IList<int> sut)
        {
            "Given a {0}".f(() => sut = (IList<int>) Activator.CreateInstance(listType));
            "When I add {2} items using {1} threads".f(() =>
            {
                var tasks = new List<Task>();
                for (var taskCount = 0; taskCount < threads; taskCount++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        for (var i = 0; i < items; i++)
                            sut.Add(i);
                    }));
                }
                Task.WaitAll(tasks.ToArray());
            });
            "Then the list should contain {3} items".f(() => sut.Count.Should().Be(expectedCount));
        }

        [Scenario]
        [Example(typeof (SimpleLockList<int>), 1, 1000, 1000)]
        [Example(typeof (SimpleLockList<int>), 2, 1000, 2000)]
        [Example(typeof (SimpleLockList<int>), 4, 1000, 4000)]
        [Example(typeof (SimpleLockList<int>), 8, 1000, 8000)]
        [Example(typeof (ReaderWriterLockList<int>), 1, 1000, 1000)]
        [Example(typeof (ReaderWriterLockList<int>), 2, 1000, 2000)]
        [Example(typeof (ReaderWriterLockList<int>), 4, 1000, 4000)]
        [Example(typeof (ReaderWriterLockList<int>), 8, 1000, 8000)]
        public void Insert(Type listType, int threads, int items, int expectedCount, IList<int> sut)
        {
            "Given a {0}".f(() => sut = (IList<int>) Activator.CreateInstance(listType));
            "When I insert {2} items using {1} threads".f(() =>
            {
                var tasks = new List<Task>();
                for (var taskCount = 0; taskCount < threads; taskCount++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        for (var i = 0; i < items; i++)
                            sut.Insert(0, i);
                    }));
                }
                Task.WaitAll(tasks.ToArray());
            });
            "Then the list should contain {3} items".f(() => sut.Count.Should().Be(expectedCount));
        }

        [Scenario]
        [Example(typeof (SimpleLockList<int>), 1000)]
        [Example(typeof (ReaderWriterLockList<int>), 1000)]
        public void ThreadSafeEnumeration(Type listType, int expected, int actual, ManualResetEventSlim signal, IList<int> sut)
        {
            "Given a {0}".f(() =>
            {
                sut = (IList<int>) Activator.CreateInstance(listType);
                for (var i = 0; i < expected; i++)
                    sut.Add(i);
            });
            "And a ManualResetEvent".f(() => signal = new ManualResetEventSlim(false));
            "When I start enumerating the list before another thread begins adding items".f(() =>
            {
                var tasks = new List<Task>();
                tasks.Add(Task.Run(() =>
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    // ReSharper disable once UnusedVariable
                    foreach (var item in sut)
                    {
                        if (!signal.IsSet) signal.Set();
                        actual++;
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    signal.Wait();
                    for (var i = 0; i < expected; i++)
                        sut.Add(i);
                }));
                Task.WaitAll(tasks.ToArray());
            });
            "Then the list should contain {1} items".f(() => actual.Should().Be(expected));
        }
    }
}