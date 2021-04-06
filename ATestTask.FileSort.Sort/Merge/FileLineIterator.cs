using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AltTestTask.FileSort.Sort.Merge
{
    public class FileLineIterator : IAsyncEnumerator<Record>, IAsyncDisposable, IDisposable
    {
        private static readonly byte[] _newLinemarker = LineFormat.NewLineByte;

        public Record Current => _current;

        private readonly Stream _stream;
        private readonly PipeReader _reader;
        private readonly long _maxBytesToConsume;

        private Record _current;
        private long _bytesConsumed;
        private int _lineCounter;

        public FileLineIterator(Stream stream, long maxBytesToConsume)
        {
            _stream = stream;
            _maxBytesToConsume = maxBytesToConsume;
            _bytesConsumed = 0;
            _current = null;

            _reader = PipeReader.Create(_stream, new StreamPipeReaderOptions(leaveOpen: true, bufferSize: 1024*1024, minimumReadSize: 1024*1024));
        }

        public async ValueTask<Record> ReadNextRecordAsync()
        {
            if (_bytesConsumed > _maxBytesToConsume)
            {
                // we've consumed all allowed bytes
                _reader.Complete();
                return null;
            }

            do
            {
                // read more data
                ReadResult result = await _reader.ReadAsync();

                if (result.IsCompleted)
                {
                    // end of the input stream reached
                    _reader.Complete();
                    return null;
                }

                ReadOnlySequence<byte> buffer = result.Buffer;
                var (newRecord, consumed) = ProcessLine(ref buffer, _lineCounter);

                if (newRecord != null)
                {
                    _bytesConsumed += consumed;
                    _reader.AdvanceTo(buffer.GetPosition(consumed, buffer.Start));

                    _lineCounter++;
                    return newRecord;
                }
                _reader.AdvanceTo(buffer.Start, buffer.End);
            }
            while (true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (Record record, long bytesConsumed) ProcessLine(ref ReadOnlySequence<byte> buffer, int counter)
        {
            var newLineMarker = _newLinemarker;

            Record record;

            if (buffer.IsSingleSegment)
            {
                var span = buffer.FirstSpan;
                if(span.Length > 0)
                {
                    var newLine = span.IndexOf(newLineMarker);

                    if (newLine == -1) return (null, 0);

                    var line = span.Slice(0, newLine);

                    record = Record.EnsureFormatAndCreate(ref line, counter);

                    return (record, line.Length + newLineMarker.Length);
                }
                return (null, 0);
            }
            else
            {
                var sequenceReader = new SequenceReader<byte>(buffer);

                if (!sequenceReader.End)
                {
                    if (sequenceReader.TryReadTo(out ReadOnlySequence<byte> line, newLineMarker))
                    {
                        record = Record.EnsureFormatAndCreate(ref line, counter);
                        return (record, line.Length + newLineMarker.Length);
                    }
                }
                return (null, 0);
            }
        }

        public void Dispose()
        {
            _reader.Complete();
            _stream.Close();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            _current = await ReadNextRecordAsync();

            return _current is not null;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public async ValueTask DisposeAsync()
        {
            await _reader.CompleteAsync();
            _stream.Close();
        }
    }
}
