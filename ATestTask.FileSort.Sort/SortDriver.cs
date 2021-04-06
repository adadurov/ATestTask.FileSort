using AltTestTask.FileSort.Sort.Merge;
using AltTestTask.FileSort.Sort.Split;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AltTestTask.FileSort.Sort
{
    public class SortDriver
    {
        private readonly string _sourceFilePath;
        private readonly int _runSizeHintInMiB;
        private readonly int _maxDegreeOfParallelism;
        private readonly Action<string> _logger;

        private readonly ExternalRunRepositoryProvider _repositoryProvider;
        private readonly FileSplitter _fileSplitter;

        public static SortDriver Create(
            string sourceFilePath,
            int runSizeHintInMiB,
            int maxDegreeOfParallelism,
            Action<string> logger)
        {
            if (!File.Exists(sourceFilePath))
                throw new ArgumentException($"File {sourceFilePath} doesn't exist.");

            return new SortDriver(sourceFilePath, runSizeHintInMiB, maxDegreeOfParallelism, logger);
        }

        private SortDriver(
            string sourceFilePath,
            int runSizeHintInMiB,
            int maxDegreeOfParallelism,
            Action<string> logger)
        {
            _sourceFilePath = sourceFilePath;
            _runSizeHintInMiB = runSizeHintInMiB;
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _logger = logger;
            _fileSplitter = new FileSplitter(logger);
            _repositoryProvider = new ExternalRunRepositoryProvider(Path.GetDirectoryName(sourceFilePath));
        }

        public async Task Sort()
        {
            string targetFileName = MakeTargetFileName(_sourceFilePath);
            if (File.Exists(targetFileName))
            {
                throw new ArgumentException($"The output file '{targetFileName}' already exist");
            }

            try
            {
                var perfReport = await DoExternalSort(_sourceFilePath, targetFileName);
                LogPerformanceInfo(perfReport);
            }
            catch
            {
                CleanUp(targetFileName);
                throw;
            }
        }

        private void LogPerformanceInfo(PerformanceReport perfReport)
        {
            var timeTaken = perfReport.TotalTime;
            var totalSeconds = timeTaken.TotalSeconds;

            _logger($"Sorted the file '{_sourceFilePath}' in {timeTaken}/{totalSeconds:0.##} seconds.");
        }

        private static string MakeTargetFileName(string pathToSourceFile)
        {
            return pathToSourceFile + ".sorted";
        }

        private async Task<PerformanceReport> DoExternalSort(string pathToSourceFile, string targetFileName)
        {
            using var runRepository = _repositoryProvider.CreateNewRepository();

            var performanceReport = new PerformanceReport();
            var stopwatch = Stopwatch.StartNew();

            await _fileSplitter.Split(
                pathToSourceFile: pathToSourceFile,
                repository: runRepository,
                runSizeHintInBytes: GetRunSizeHint(),
                lineLengthHintInBytes: 24,
                maxDegreeOfParallelism: GetDegreeOfParallelism()
                );

            stopwatch.Stop();
            performanceReport.SplitTime = stopwatch.Elapsed;

            stopwatch = Stopwatch.StartNew();

            await MergeSortedRuns(runRepository, targetFileName);
            stopwatch.Stop();
            performanceReport.MergeTime = stopwatch.Elapsed;

            return performanceReport;
        }

        private int GetDegreeOfParallelism()
        {
            if (_maxDegreeOfParallelism == 0)
            {
                // use all available CPU cores (mind memory consumption)
                return Environment.ProcessorCount;
            }
            if (_maxDegreeOfParallelism < 0)
            {
                // unlimited, sort all runs in parallel -- no point using this to do external sort
                return -1;
            }
            return _maxDegreeOfParallelism;
        }

        private int GetRunSizeHint()
        {
            var maxBytesPerRunHint = Int32.MaxValue;
            var requestedRunSize = _runSizeHintInMiB * (int)Units.BytesPerMiB;

            if (requestedRunSize > maxBytesPerRunHint)
            {
                return maxBytesPerRunHint;
            }

            return requestedRunSize;
        }

        private async Task MergeSortedRuns(ExternalRunRepository runRepository, string targetFileName)
        {
            _logger($"Started merge on {runRepository.Count} runs");

            using var runs = runRepository.GetStoredRunsAsIterableRecords();

            await StreamMerger.Merge(
                runs: runs,
                targetFileName: targetFileName,
                logger: _logger);
        }

        private static void CleanUp(string targetFileName)
        {
            File.Delete(targetFileName);
        }
    }
}
