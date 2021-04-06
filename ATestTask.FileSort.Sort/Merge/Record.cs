using System;
using System.Buffers;

namespace AltTestTask.FileSort.Sort.Merge
{
    public record Record
    {
        private static readonly byte[] EmptyLine = Array.Empty<byte>();

        public static Record Empty() => new Record(EmptyLine, 0);

        public static Record EnsureFormatAndCreate(ref ReadOnlySpan<byte> line, long lineIndex)
        {
            var buffer = new byte[line.Length];
            line.CopyTo(buffer);

            var pos = buffer.AsSpan().IndexOf((byte)'.');

            if (pos < 0)
            {
                throw new ArgumentException($"The line #{lineIndex} is not properly formatted.");
            }

            return new Record(buffer, pos + 2);
        }

        public static Record EnsureFormatAndCreate(ref ReadOnlySequence<byte> line, long lineIndex)
        {
            var buffer = new byte[line.Length];
            line.CopyTo(buffer);

            var pos = buffer.AsSpan().IndexOf((byte)'.');

            if (pos < 0)
            {
                throw new ArgumentException($"The line #{lineIndex} is not properly formatted.");
            }

            return new Record(buffer, pos + 2);
        }


        private Record(byte[] line, int textStart)
        {
            _line = line;
            _textStart = textStart;
        }

        public readonly int _textStart;
        public readonly byte[] _line;
    }
}
