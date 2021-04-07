using ATestTask.FileSort.Sort.Merge;
using System;
using System.Collections.Generic;
using System.IO;

namespace ATestTask.FileSort.Sort
{
    public sealed class ExternalRunRepository : IDisposable
    {
        private readonly string _targetDirectory;
        private readonly List<string> _files;

        public ExternalRunRepository(string targetDirectory)
        {
            _files = new List<string>(100);
            _targetDirectory = targetDirectory;
            Directory.CreateDirectory(targetDirectory);
        }

        public int Count => _files.Count;

        public void Dispose()
        {
            Directory.Delete(_targetDirectory, true);
        }

        public ExternalRunCollection GetStoredRunsAsIterableRecords(Func<FileStream, IAsyncDisposableEnumerator<Record>> iteratorFactory)
        {
            return new ExternalRunCollection(_files, iteratorFactory);
        }

        public string Rent()
        {
            lock (this)
            {
                var fileName = Path.Combine(_targetDirectory, Count.ToString() + ".tmp");
                _files.Add(fileName);
                return fileName;
            }
        }
    }
}
