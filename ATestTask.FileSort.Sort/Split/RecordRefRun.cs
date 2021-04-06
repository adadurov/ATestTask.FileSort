using System;
using System.Collections.Generic;

namespace AltTestTask.FileSort.Sort.Split
{
    public class RecordRefRun
    {
        public List<RecordRef> Records { get; set; }

        public byte[] Buffer { get; set; }

        internal int Limit { get; set; }

        public void Aggregate(long v, Func<object, object, object> p)
        {
            throw new NotImplementedException();
        }
    }
}
