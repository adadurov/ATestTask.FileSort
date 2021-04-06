using System;

namespace AltTestTask.FileSort.Sort.Split
{
    public record RecordRef
    {
        public static RecordRef EnsureFormatAndCreate(ref ReadOnlySpan<byte> owningRun, int offset, short length, long lineIndex)
        {
            var pos = owningRun.Slice(offset, length).IndexOf((byte)'.');

            if (pos < 0 || pos > length)
            {
                throw new ArgumentException($"The line #{lineIndex} of length {length} is not properly formatted.");
            }

            return new RecordRef(offset, length, (short)(pos + 2));
        }

        private RecordRef(int offset, short length, short textStart)
        {
            this.offset = offset;
            this.length = length;
            this.textStart = textStart;
        }

        public readonly int offset;
        public readonly short length;
        public readonly short textStart;
    }
}
