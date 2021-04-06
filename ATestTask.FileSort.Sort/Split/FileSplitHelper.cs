using System;
using System.Collections.Generic;
using System.IO;

namespace AltTestTask.FileSort.Sort.Split
{
    public class FileSplitHelper
    {
        private readonly FileInfo _fileInfo;
        private readonly Action<string> _logger;

        public FileSplitHelper(FileInfo fileInfo, Action<string> logger)
        {
            _fileInfo = fileInfo;
            _logger = logger;
        }

        public List<(long offset, int length)> GetRuns(int runSizeHint)
        {
            using var fileStream = new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read);

            _logger($"Identifying splitting points for file {_fileInfo.FullName} with runs of ~{runSizeHint / Units.BytesPerMiB} MiB...");
            var runs = new List<(long offset, int length)>((int)(_fileInfo.Length / runSizeHint) + 1);

            fileStream.Seek(0, SeekOrigin.Begin);
            long startOffset = 0;

            while (fileStream.Position < fileStream.Length)
            {
                if( fileStream.Position + runSizeHint >= fileStream.Length)
                {
                    if (fileStream.Length - fileStream.Position > 0)
                    {
                        runs.Add((startOffset, (int)(fileStream.Length - fileStream.Position)));
                    }
                    break;
                }
                fileStream.Seek(runSizeHint, SeekOrigin.Current);

                while (fileStream.Position < fileStream.Length && fileStream.ReadByte() != (byte)'\n') ;

                runs.Add((startOffset, (int)(fileStream.Position-startOffset)));
                startOffset = fileStream.Position;
            }

            _logger($"The file will be split into {runs.Count} runs");

            return runs;
        }
    }
}