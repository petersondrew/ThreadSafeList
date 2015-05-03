using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Binarysharp.Benchmark;
using FluentAssertions;
using Xunit;

namespace List
{
    public class PerformanceTest
    {
        const int ReadOperations = 10;
        const int AddOperations = 10000;

        static readonly IEnumerable<int> ReadTestData = Enumerable.Range(0, ReadOperations);

        static readonly Action<int> SimpleLockAddTest = threadCount => Add(threadCount, AddOperations, new SimpleLockList<int>());
        static readonly Action<int> ReaderWriterLockAddTest = threadCount => Add(threadCount, AddOperations, new ReaderWriterLockList<int>());

        static readonly Action<int, IList<int>> SimpleLockEnumerationTest = (threadCount, list) => Enumerate(threadCount, list);
        static readonly Action<int, IList<int>> ReaderWriterLockEnumerationTest = (threadCount, list) => Enumerate(threadCount, list);

        public PerformanceTest()
        {
            // Print results in Xunit output regardless of pass/fail
            Debug.Listeners.Add(new DefaultTraceListener());
        }

        [Fact]
        public void ThreadSafeLockShouldAddFaster() { AddTest(Environment.ProcessorCount); }

        [Fact]
        public void ReaderWriterLockShouldReadFaster() { ReadTest(Environment.ProcessorCount); }

        static void AddTest(int threadCount)
        {
            var shark = new BenchShark();
            var threadSafeLockResults = shark.EvaluateTask(() => SimpleLockAddTest(threadCount), 100);
            var readerWriterLockResults = shark.EvaluateTask(() => ReaderWriterLockAddTest(threadCount), 100);
            Debug.WriteLine("SimpleLock: {0}; ReaderWriterLock: {1}", threadSafeLockResults.AverageExecutionTime, readerWriterLockResults.AverageExecutionTime);
            threadSafeLockResults.AverageExecutionTime.Should().BeLessThan(readerWriterLockResults.AverageExecutionTime);
        }

        static void ReadTest(int threadCount)
        {
            var shark = new BenchShark();
            var threadSafeLockResults = shark.EvaluateTask(() => SimpleLockEnumerationTest(threadCount, new SimpleLockList<int>(ReadTestData)), 100);
            var readerWriterLockResults = shark.EvaluateTask(() => ReaderWriterLockEnumerationTest(threadCount, new ReaderWriterLockList<int>(ReadTestData)), 100);
            Debug.WriteLine("SimpleLock: {0}; ReaderWriterLock: {1}", threadSafeLockResults.AverageExecutionTime, readerWriterLockResults.AverageExecutionTime);
            readerWriterLockResults.AverageExecutionTime.Should().BeLessThan(threadSafeLockResults.AverageExecutionTime);
        }

        static void Add(int threads, int items, ICollection<int> sut)
        {
            var threadList = new List<Thread>();
            for (var taskCount = 0; taskCount < threads; taskCount++)
            {
                var thread = new Thread(() =>
                {
                    for (var i = 0; i < items; i++)
                        sut.Add(i);
                });
                thread.Start();
                threadList.Add(thread);
            }
            threadList.ForEach(t => t.Join());
        }

        static void Enumerate(int threads, IEnumerable<int> sut)
        {
            var threadList = new List<Thread>();
            for (var taskCount = 0; taskCount < threads; taskCount++)
            {
                var thread = new Thread(() =>
                {
                    // Simulate work during enumeration
                    foreach (var i in sut)
                        Thread.Sleep(1);
                });
                thread.Start();
                threadList.Add(thread);
            }
            threadList.ForEach(t => t.Join());
        }
    }
}