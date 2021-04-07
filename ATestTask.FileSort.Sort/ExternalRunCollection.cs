using ATestTask.FileSort.Sort.Merge;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ATestTask.FileSort.Sort
{

    public interface IAsyncDisposableEnumerator<T> : IAsyncEnumerator<T>, IAsyncDisposable, IDisposable { }


    public sealed class ExternalRunCollection : IDisposable, IEnumerable<IAsyncEnumerator<Record>>
    {
        private List<IAsyncDisposableEnumerator<Record>> _recordStreams;

        public ExternalRunCollection(IEnumerable<string> fileNames, Func<FileStream, IAsyncDisposableEnumerator<Record>> iteratorFactory)
        {
            _recordStreams = fileNames.Select(file => {
                var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                return iteratorFactory(fileStream);
            }).ToList();
        }

        public void Dispose()
        {
            foreach( var stream in _recordStreams)
            {
                stream.Dispose();
            }
        }

        public IEnumerator<IAsyncEnumerator<Record>> GetEnumerator()
        {
            return _recordStreams.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _recordStreams.GetEnumerator();
        }
    }
}
