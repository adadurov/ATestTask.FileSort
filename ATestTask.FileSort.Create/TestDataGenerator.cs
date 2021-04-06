using AltTestTask.FileSort.Create.Streams;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AltTestTask.FileSort.Create
{
    internal class TestDataGenerator
    {
        private class WriterState
        {
            public long WriteRequests;

            public long RequestedFileSizeInBytes;
        }

        private class WriterWorkItem
        {
            /// <summary>
            /// The buffer is allocated from ArrayPool<byte> 
            /// and must be returned to the corresponding pool once processing is completed
            /// to keep memory consumption under control
            /// </summary>
            public byte[] PooledBuffer;

            public int ValidSize;
        }

        private readonly int _outputStreamBufferInBytes;
        private readonly Func<ChunkStream> _chunkStreamFactory;
        private readonly string _targetFilePath;
        private readonly long _targetFileSizeInBytes;
        private readonly int _maxProducerConcurrency;
        private readonly int _maxChunkSizeInBytes;

        private readonly int _maxQueuedChunks;
        private readonly Action<string> _logger;

        public TestDataGenerator(
            string targetFilePath, 
            long targetFileSizeInBytes,
            int maxGeneratorsConcurrency,
            Func<ChunkStream> chunkStreamFactory,
            int maxChunkSizeInBytes,
            int outputFileBufferLength,
            Action<string> logger)
        {
            _chunkStreamFactory = chunkStreamFactory;
            _targetFilePath = targetFilePath;
            _targetFileSizeInBytes = targetFileSizeInBytes;
            _maxProducerConcurrency = maxGeneratorsConcurrency;
            _maxQueuedChunks = maxGeneratorsConcurrency * 2;
            _maxChunkSizeInBytes = maxChunkSizeInBytes;
            _outputStreamBufferInBytes = outputFileBufferLength;
            _logger = logger;
        }

        internal async Task<PerformanceReport> Generate()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var (bytesWritten, ios) = await GenerateInternalAndReturnBytesWritten();

            stopwatch.Stop();

            return new PerformanceReport {
                RealSize = bytesWritten,
                TimeTaken = stopwatch.Elapsed,
                WriteRequests = ios
            };
        }

        private async Task<(long size, long ios)> GenerateInternalAndReturnBytesWritten()
        {
            var chunkQueue = new BlockingCollection<WriterWorkItem>(_maxQueuedChunks);
            var state = new WriterState
            {
                RequestedFileSizeInBytes = _targetFileSizeInBytes
            };

            var bufferPool = ArrayPool<byte>.Shared;

            _logger($"Writing {_targetFileSizeInBytes/Units.BytesPerMiB} MiB of test data...");
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();

                var consumerTasks = LaunchChunkWriters(state, chunkQueue, bufferPool);
                var producerTasks = LaunchChunkProducers(chunkQueue, bufferPool, cancellationTokenSource.Token);

                var allTasks = producerTasks.Concat(consumerTasks);

                var result = await Task.WhenAny(allTasks);
                cancellationTokenSource.Cancel();
                ThrowIfFailed(result);
                await Task.WhenAll(allTasks);

                return (GetActualFileSize(), state.WriteRequests);
            }
            finally
            {
                chunkQueue?.CompleteAdding();
            }
        }

        private static void ThrowIfFailed(Task result)
        {
            if (result.IsFaulted)
            {
                var ex = result.Exception;
                throw new Exception(
                    "An error occurred while writing test data: " + ex?.Message,
                    ex);
            }
        }

        private long GetActualFileSize()
        {
            var fi = new FileInfo(_targetFilePath);
            return fi.Length;
        }

        private IEnumerable<Task> LaunchChunkWriters(WriterState state, BlockingCollection<WriterWorkItem> chunkQueue, ArrayPool<byte> bufferPool)
        {
            // WARNING! We are not ready for concurrent writers/generators!
            return Enumerable.Repeat(Task.Run(async () => await WriterRoutine(state, chunkQueue, CreateTargetFile, bufferPool)), 1);
        }

        private IEnumerable<Task> LaunchChunkProducers(BlockingCollection<WriterWorkItem> chunkQueue, ArrayPool<byte> bufferPool, CancellationToken token)
        {
            return Enumerable
                .Range(1, _maxProducerConcurrency)
                .Select(i => Task.Run(async () => await ProducerRoutine(chunkQueue, _chunkStreamFactory, _maxChunkSizeInBytes, bufferPool, token)));
        }

        private static Task ProducerRoutine(
            BlockingCollection<WriterWorkItem> chunkQueue, Func<ChunkStream> chunkStreamFactory, int maxChunkSizeInBytes, ArrayPool<byte> bufferPool, CancellationToken cancellationToken)
        {
            var chunkStream = chunkStreamFactory();
            while (!cancellationToken.IsCancellationRequested)
            {
                var buffer = bufferPool.Rent(maxChunkSizeInBytes);
                var validDataLength = chunkStream.CopyNextChunk(buffer);

                chunkQueue.Add(new WriterWorkItem { PooledBuffer = buffer, ValidSize = validDataLength }, cancellationToken);
            }
            return Task.CompletedTask;
        }

        private static async Task WriterRoutine(
            WriterState state,
            BlockingCollection<WriterWorkItem> chunkSource,
            Func<FileStream> getOutputStream,
            ArrayPool<byte> bufferPool)
        {
            using var outputStream = getOutputStream();

            foreach (var item in chunkSource.GetConsumingEnumerable())
            {
                try
                {
                    await outputStream.WriteAsync(item.PooledBuffer, 0, item.ValidSize);
                    ++state.WriteRequests;
                    if (outputStream.Position > state.RequestedFileSizeInBytes)
                    {
                        return;
                    }
                }
                finally
                {
                    bufferPool.Return(item.PooledBuffer);
                }
            }
        }

        private FileStream CreateTargetFile()
        {
            return new FileStream(
                        _targetFilePath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.ReadWrite,
                        _outputStreamBufferInBytes
                        );
        }
    }
}
