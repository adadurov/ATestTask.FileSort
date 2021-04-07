using System.Collections.Generic;

namespace ATestTask.FileSort.Create.Streams
{
    /// <summary>
    /// produces a stream of words based on a subset of a dictionary sequentially
    /// </summary>
    internal class WordStream
    {
        readonly IList<string> _dictionary;
        readonly int _offset;
        readonly int _count;
        int index;

        internal WordStream(IList<string> dictionary, int offset, int count)
        {
            _dictionary = dictionary;
            _offset = offset;
            _count = count;
        }

        public string NextWord()
        {
            index++;
            index %= _count;
            return _dictionary[_offset + index];
        }
    }
}
