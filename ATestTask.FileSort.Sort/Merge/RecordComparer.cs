using System.Runtime.CompilerServices;

namespace ATestTask.FileSort.Sort.Merge
{
    public static class RecordComparer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Min(int v1, int v2) => v1 < v2 ? v1 : v2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(Record r1, Record r2)
        {
            int d;
            var tl1 = r1._line.Length - r1._textStart;
            var tl2 = r2._line.Length - r2._textStart;
            var len = Min(tl1, tl2);
            // compare text
            for (int i = 0; i < len; ++i)
            {
                d = r1._line[r1._textStart + i] - r2._line[r2._textStart + i];
                if (d == 0) continue;
                return d;
            }
            var textLengthCompareResult = tl1.CompareTo(tl2);
            if (textLengthCompareResult != 0) return textLengthCompareResult;

            // compare number
            // assume there are no leading zeroes
            // (if there are any, this method may yield wrong result)
            d = r1._textStart - r2._textStart;
            if (d != 0) return d;

            var numLen = r1._textStart - 2;
            for (int i = 0; i < numLen; ++i)
            {
                d = r1._line[i] - r2._line[i];
                if (d == 0) continue;
                return d;
            }
            return 0;
        }
    }
}
