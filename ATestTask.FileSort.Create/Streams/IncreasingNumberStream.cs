using System;
using System.Text;

namespace ATestTask.FileSort.Create.Streams
{
    /// <summary>
    /// produces a stream of increasing numbers from a random positive Int32 value to System.UInt64.MaxValue
    /// and then wraps around to 0
    /// </summary>
    class IncreasingNumberStream
    {
        public IncreasingNumberStream()
        {
            var rndGen = new Random((int)DateTime.Now.Ticks);
            _number = (UInt64)rndGen.Next(0, int.MaxValue);
        }
        private UInt64 _number;

        public void NextNumber(StringBuilder _sb)
        {
            if (_number == UInt64.MaxValue)
            {
                _number = 0;
            }

            ++_number;
            _sb.Append(_number);
        }
    }
}
