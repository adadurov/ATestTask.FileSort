using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ATestTask.FileSort.Sort.Merge
{
    public sealed class DoublyBufferedLineIterator : IAsyncDisposableEnumerator<Record>
    {
        private static readonly byte[] _newLineMarker = LineFormat.NewLineByte;

        private readonly Stream _stream;
        private readonly PipeReader _pipeReader;
        private readonly int _bufferSize;
        private Record _current;
        private int _lineCounter;

        private readonly ArrayBufferWriter<byte> _buffer1;
        private readonly ArrayBufferWriter<byte> _buffer2;
        private ArrayBufferWriter<byte> _workBuffer;
        private ArrayBufferWriter<byte> _ioBuffer;
        private int _nextWorkLocation = 0;

        private Task<bool> _backgroundReadTask;

        public DoublyBufferedLineIterator(Stream stream, int bufferSize)
        {
            _stream = stream;
            _bufferSize = bufferSize;
            _current = null;

            _pipeReader = PipeReader.Create(_stream, new StreamPipeReaderOptions(leaveOpen: true, bufferSize: _bufferSize, minimumReadSize: _bufferSize));

            _buffer1 = new ArrayBufferWriter<byte>(bufferSize);
            _buffer2 = new ArrayBufferWriter<byte>(bufferSize);
            _workBuffer = _buffer1;
            _ioBuffer = _buffer2;

            _nextWorkLocation = 0;

            StartBackgroundReadTask();
        }

        public Record Current => _current;

        public void Dispose()
        {
            _pipeReader.Complete();
            _stream.Close();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            _current = await ReadNextRecordAsync();

            return _current is not null;
        }

        public async ValueTask<Record> ReadNextRecordAsync()
        {
            {
                var record = GetNextRecordFromWorkBuffer();
                if (record is not null) return record;
            }

            do
            {
                // first, wait for completion of the read request
                // and check whether the stream has been read until EOF
                var streamCompleted = await _backgroundReadTask.ConfigureAwait(false);

                if (streamCompleted )
                {
                    // end of the input stream reached
                    _pipeReader.Complete();
                    return null;
                }

                SwitchBuffers();

                StartBackgroundReadTask();

                // try to create a record
                var record = GetNextRecordFromWorkBuffer();
                if (record is not null)
                {
                    return record;
                }
            }
            while (true);
        }

        private void SwitchBuffers()
        {
            var buf = _ioBuffer;
            _ioBuffer = _workBuffer;
            _workBuffer = buf;
            _nextWorkLocation = 0;
        }

        private void StartBackgroundReadTask()
        {
            _backgroundReadTask = _pipeReader
                .ReadAsync()
                .AsTask()
                .ContinueWith(t => CompleteReading(t.Result, _ioBuffer, _pipeReader));
        }

        private static bool CompleteReading(ReadResult result, ArrayBufferWriter<byte> ioBuffer, PipeReader pipeReader)
        {
            if (result.IsCompleted) return true;
            CopyFromPipeBufferUntilLastEOL(result.Buffer, pipeReader, ioBuffer);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Record GetNextRecordFromWorkBuffer()
        {
            if (_nextWorkLocation >= _workBuffer.WrittenCount) return null;

            var workSpan = _workBuffer.WrittenSpan[_nextWorkLocation..];
            // read current buffer until the next '\n' character and return a Record
            var eolPosition = workSpan.IndexOf(LineFormat.PartsSeparatorBytes);
            if (eolPosition > 0)
            {
                // make a new record
                var lineSpan = workSpan[..eolPosition];
                var record = Record.EnsureFormatAndCreate(ref lineSpan, _lineCounter++);

                _nextWorkLocation += eolPosition + LineFormat.PartsSeparatorBytes.Length;
                return record;
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long CopyFromPipeBufferUntilLastEOL(
            ReadOnlySequence<byte> buffer,
            PipeReader _pipeReader,
            ArrayBufferWriter<byte> targetBuffer)
        {
            targetBuffer.Clear();
            if (buffer.IsSingleSegment)
            {
                var length = buffer.FirstSpan.LastIndexOf(_newLineMarker) + 1;
                targetBuffer.Write(buffer.FirstSpan[..length]);
                _pipeReader.AdvanceTo(buffer.GetPosition(length), buffer.End);
                return length;
            }
            else
            {
                var sequenceReader = new SequenceReader<byte>(buffer);
                if (!sequenceReader.End)
                {
                    while (sequenceReader.TryReadTo(out ReadOnlySequence<byte> line, _newLineMarker))
                    {
                        if (line.Length >= Int32.MaxValue)
                        {
                            throw new OutOfMemoryException("The input is too long");
                        }

                        line.CopyTo(targetBuffer.GetSpan((int)line.Length));
                        targetBuffer.Advance((int)line.Length);
                        targetBuffer.Write(_newLineMarker);
                    }
                }
                _pipeReader.AdvanceTo(buffer.GetPosition(targetBuffer.WrittenCount), buffer.End);
            }
            return targetBuffer.WrittenCount;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public async ValueTask DisposeAsync()
        {
            

            await _pipeReader.CompleteAsync();
            _stream.Close();
        }
    }
}
