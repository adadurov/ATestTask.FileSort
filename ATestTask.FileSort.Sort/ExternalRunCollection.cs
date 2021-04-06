using AltTestTask.FileSort.Sort.Merge;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AltTestTask.FileSort.Sort
{
    public class ExternalRunCollection : IDisposable, IEnumerable<FileLineIterator>
    {
        private List<FileLineIterator> _recordStreams;

        public ExternalRunCollection(IEnumerable<string> fileNames)
        {
            _recordStreams = fileNames.Select(file => {
                var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                return new FileLineIterator(fileStream, maxBytesToConsume: long.MaxValue);
            }).ToList();
        }

        public void Dispose()
        {
            foreach( var stream in _recordStreams)
            {
                stream.Dispose();
            }
        }

        public IEnumerator<FileLineIterator> GetEnumerator()
        {
            return _recordStreams.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _recordStreams.GetEnumerator();
        }
    }
}
