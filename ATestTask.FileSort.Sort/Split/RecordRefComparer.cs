using System;
using System.Runtime.CompilerServices;

namespace ATestTask.FileSort.Sort.Split
{
    public class RecordRefComparer
    {
        private byte[] buffer;
        private int limit;

        public RecordRefComparer(byte[] buffer, int limit)
        {
            this.buffer = buffer;
            this.limit = limit;
        }
  
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int v1, int v2) => v1 < v2 ? v1 : v2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(RecordRef r1, RecordRef r2)
        {
            var owningRun = buffer.AsSpan(0, limit);

            var r1Span = owningRun.Slice(r1.offset, r1.length);
            var r2Span = owningRun.Slice(r2.offset, r2.length);

            int d;
            var tl1 = r1.length - r1.textStart;
            var tl2 = r2.length - r2.textStart;
            var len = Min(tl1, tl2);
            
            // compare text
            for (int i = 0; i < len; ++i)
            {
                d = r1Span[r1.textStart + i] - r2Span[r2.textStart + i];
                if (d == 0) continue;
                return d;
            }
            var textLengthCompareResult = tl1.CompareTo(tl2);
            if (textLengthCompareResult != 0) return textLengthCompareResult;

            // compare number
            // assume there are no leading zeroes
            // (if there are any, this method may yield wrong result)
            d = r1.textStart - r2.textStart;
            if (d != 0) return d;

            var numLen = r1.textStart - 2;
            for (int i = 0; i < numLen; ++i)
            {
                d = r1Span[i] - r2Span[i];
                if (d == 0) continue;
                return d;
            }
            return 0;
        }
    }
}
