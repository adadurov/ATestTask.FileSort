using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ATestTask.FileSort.Sort.Split
{
    public class RunSorterInPlace
    {
        private readonly FileInfo _inputFileInfo;
        private readonly long _offset;
        private readonly int _length;
        private readonly int _lineLengthHintInBytes;

        public RunSorterInPlace(FileInfo inputFileInfo, long offset, int length, int lineLengthHintInBytes)
        {
            _inputFileInfo = inputFileInfo;
            _offset = offset;
            _length = length;
            _lineLengthHintInBytes = lineLengthHintInBytes;
        }

        public async Task<RecordRefRun> Read()
        {
            using var fileStream = new FileStream(_inputFileInfo.FullName, FileMode.Open, FileAccess.Read);
            fileStream.Seek(_offset, SeekOrigin.Begin);

            byte[] buffer = null;

            buffer = RentBuffer(_length);
            var limit = await fileStream.ReadAsync(buffer);
            var items = ExtractItems(buffer, limit);

            return new RecordRefRun
            {
                Buffer = buffer,
                Limit = limit,
                Records = items
            };
        }

        public void Sort(RecordRefRun run)
        {
            var comparer = new RecordRefComparer(run.Buffer, run.Limit);

            run.Records.Sort(comparer.Compare);
        }

        public async Task Save(RecordRefRun run, string targetFileName, bool returnBuffer = true)
        {
            using var output = new FileStream(
                                path: targetFileName,
                                FileMode.CreateNew,
                                FileAccess.Write,
                                FileShare.None,
                                bufferSize: 1024 * (int)Units.BytesPerKiB);

            const int capacity = 900 * 1000;

            ValueTask writeTask = ValueTask.CompletedTask;

            var buffer1 = new ArrayBufferWriter<byte>(capacity);
            var buffer2 = new ArrayBufferWriter<byte>(capacity);

            var buffer = buffer1;
            foreach (var item in run.Records)
            {
                if (buffer.FreeCapacity < item.length)
                {
                    await writeTask;
                    writeTask = output.WriteAsync(buffer.WrittenMemory);

                    buffer = buffer == buffer1 ? buffer2 : buffer1;
                    buffer.Clear();
                }
                buffer.Write(run.Buffer.AsSpan().Slice(item.offset, item.length));
            }
            await writeTask;
            await output.WriteAsync(buffer.WrittenMemory);

            if(returnBuffer)
            {
                ReturnBuffer(run.Buffer);
            }
        }

        private byte[] RentBuffer(int size)
        {
            return ArrayPool<byte>.Shared.Rent(_length);
        }

        private void ReturnBuffer(byte[] buffer)
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        private List<RecordRef> ExtractItems(byte[] buffer, int limit)
        {
            var span = new ReadOnlySpan<byte>(buffer, 0, limit);
            var items = new List<RecordRef>(span.Length / _lineLengthHintInBytes + 1);

            int i = 0;
            int lastStart = 0;
            int recordCounter = 0;
            while (i < span.Length)
            {
                while (i < span.Length && span[i] != (byte)'\n') ++i;
                var record = RecordRef.EnsureFormatAndCreate(ref span, lastStart, (short)(i - lastStart + 1), ++recordCounter);

                lastStart = ++i;
                items.Add(record);
            }

            return items;
        }
    }
}
