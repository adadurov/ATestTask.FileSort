using System;
using System.IO;

namespace AltTestTask.FileSort.Sort
{
    public class ExternalRunRepositoryProvider
    {
        private readonly string _directoryName;

        public ExternalRunRepositoryProvider(string directoryName)
        {
            _directoryName = directoryName;
        }

        public ExternalRunRepository CreateNewRepository()
        {
            return new ExternalRunRepository(GetNewTempDirName());
        }

        private string GetNewTempDirName()
        {
            var path = _directoryName;
            var tempDirName = Path.GetRandomFileName();

            var tempDirPath = Path.Combine(path, tempDirName);
            if (Directory.Exists(tempDirName))
            {
                throw new Exception($"Temp directory '{tempDirPath}' already exists. Please clean up and try again");
            }
            return tempDirPath;
        }
    }
}