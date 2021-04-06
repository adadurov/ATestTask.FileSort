using System;
using System.Text;

namespace AltTestTask.FileSort.Create.Streams
{
    /// <summary>
    /// a stream of strings that produces strings, each of which contains a chunk of lines
    /// formatted as the test data
    /// </summary>
    class ChunkStream
    {
        private readonly LineStream _lineStream;

        private readonly Encoding _encoding;
        private readonly byte[] _newline;

        public ChunkStream(LineStream lineStream, Encoding encoding)
        {
            _lineStream = lineStream;
            _encoding = encoding;
            _newline = _encoding.GetBytes(Environment.NewLine);
        }

        public int CopyNextChunk(byte[] targetBuffer)
        {
            var offset = 0;
            while (true)
            {
                var line = _lineStream.NextLine();

                var bytesCopied = AppendLine(targetBuffer, offset, line);
                if (bytesCopied == 0)
                {
                    return offset;
                }

                offset += bytesCopied;
            }
        }

        private int AppendLine(byte[] buffer, int offset, string line)
        {
            // try to return fast when we have too little space instead of 
            // relying on the slow exception handling in Encoding
            if (buffer.Length - offset < (line.Length + 2) * 2) return 0;

            try
            {
                var textBytes = _encoding.GetBytes(line, 0, line.Length, buffer, offset);

                if (offset + textBytes + _newline.Length > buffer.Length) 
                {
                    return 0;
                }

                Array.Copy(_newline, 0, buffer, offset + textBytes, _newline.Length);

                return textBytes + _newline.Length;
            }
            catch (ArgumentException)
            {
                // buffer doesn't have enough capacity from index offset to hold the resulting bytes
                return 0;
            }
        }
    }
}
