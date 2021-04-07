using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace ATestTask.FileSort.Create.Streams
{
    /// <summary>
    /// produces a stream of lines in the required format
    /// </summary>
    class LineStream
    {
        readonly Action<StringBuilder> _numberApender;
        readonly Func<string>[] _wordGenerators;
        readonly StringBuilder _sb;
        int _numWords = 0;

        public LineStream(Action<StringBuilder> numberAppender, Func<string>[] wordGenerators)
        {
            Contract.Requires(numberAppender != null, $"{nameof(numberAppender)} must not be null");
            Contract.Requires(wordGenerators != null, $"{nameof(wordGenerators)} must not be null");

            foreach(var wordGen in wordGenerators)
            {
                Contract.Requires(wordGen != null, $"Each item in the ${nameof(wordGenerators)} array must be non-null");
            }

            _numberApender = numberAppender;
            _wordGenerators = wordGenerators;

            // magic number -- BAD! BAD! BAD! 
            // initial size hint
            _sb = new StringBuilder(100);
        }

        private void AppendNextWords(StringBuilder _sb)
        {
            int numWords = GetNumWords();

            foreach (var generator in _wordGenerators.Take(numWords))
            {
                _sb.Append(" ");
                _sb.Append(generator());
            }
        }

        private int GetNumWords()
        {
            _numWords++;
            _numWords %= _wordGenerators.Length;
            return _numWords + 1;
        }

        public string NextLine()
        {
            _sb.Clear();

            _numberApender(_sb);
            _sb.Append(".");

            AppendNextWords(_sb);

            return _sb.ToString();
        }
    }
}
