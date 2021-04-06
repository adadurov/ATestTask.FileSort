using System;
using System.Collections.Generic;
using System.IO;

namespace AltTestTask.FileSort.Sort
{
    public class ExternalRunRepository : IDisposable
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

        public ExternalRunCollection GetStoredRunsAsIterableRecords()
        {
            return new ExternalRunCollection(_files);
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
