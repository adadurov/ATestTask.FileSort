using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AltTestTask.FileSort.Sort.Merge
{
    public class StreamMerger
    {
        private static readonly long HundredMegabytes = 100 * Units.BytesPerKiB * Units.BytesPerKiB;

        public static async Task Merge(IEnumerable<IAsyncEnumerator<Record>> runs, string targetFileName, Action<string> logger)
        {
            using var output = new FileStream(path: targetFileName, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

            var newLineBytes = LineFormat.NewLineByte;
            const int maxMergeChunkSize = 900 * 1000;
            ValueTask writeTask = ValueTask.CompletedTask;

            var buffer1 = new ArrayBufferWriter<byte>(maxMergeChunkSize);
            var buffer2 = new ArrayBufferWriter<byte>(maxMergeChunkSize);
            var buffer = buffer1;

            var heap = await InitPriorityQueue(runs);

            var lastSize = 0L;
            while (heap.Count > 0)
            {
                var inputWithNextSmallest = heap.Remove();

                var record = inputWithNextSmallest.Current;

                if (buffer.FreeCapacity < record._line.Length + newLineBytes.Length)
                {
                    // start writing out the almost-full buffer
                    await writeTask;
                    writeTask = output.WriteAsync(buffer.WrittenMemory);
                    // switch over to the other buffer
                    buffer = buffer == buffer1 ? buffer2 : buffer1;
                    buffer.Clear();

                    lastSize = LogSizeIncrease(logger, targetFileName, output.Length, lastSize);
                }

                buffer.Write(record._line);
                buffer.Write(newLineBytes);

                var hasMoreLines = await inputWithNextSmallest.MoveNextAsync();

                if (hasMoreLines)
                {
                    heap.Insert(inputWithNextSmallest);
                }
            }

            if (buffer.WrittenCount > 0)
            {
                await writeTask;
                await output.WriteAsync(buffer.WrittenMemory);
            }
        }

        private static long LogSizeIncrease(Action<string> logger, string targetFileName, long outputLength, long lastSize)
        {
            var newSize = outputLength / HundredMegabytes;
            if (newSize > lastSize)
            {
                logger($"{newSize * 100} MB written to {targetFileName}");
                lastSize = newSize;
            }
            return lastSize;
        }

        private static async Task<Heap<IAsyncEnumerator<Record>>> InitPriorityQueue(IEnumerable<IAsyncEnumerator<Record>> inputs)
        {
            var heap = new Heap<IAsyncEnumerator<Record>>((e1, e2) => RecordComparer.Compare(e1.Current, e2.Current));
            foreach (var input in inputs)
            {
                var hasNext = await input.MoveNextAsync();
                if (hasNext)
                {
                    heap.Insert(input);
                }
            }

            return heap;
        }
    }
}
