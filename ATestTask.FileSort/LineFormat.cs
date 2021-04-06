using System;

namespace AltTestTask.FileSort
{
    public static class LineFormat
    {
        private static readonly byte[] _newLineByte = new byte[] { (byte)'\n' };

        public static byte[] NewLineByte => _newLineByte;

        public static string PartsSeparator => ". ";

        public static ReadOnlySpan<byte> PartsSeparatorBytes => new byte[] { (byte)'.', (byte)' ' }.AsSpan();
    }
}
