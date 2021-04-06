using AltTestTask.FileSort.Sort.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AltTestTask.FileSort.Tests.Sort.Merge
{
    [TestClass]
    public class FileLineIterator_Test
    {
        public string OneGigFileName => @"f:\atesttask\G001.txt";

        private static FileLineIterator CreateIterator(FileStream fileStream)
        {
            return new FileLineIterator(
                fileStream,
                maxBytesToConsume: long.MaxValue
            );
        }

        [TestMethod]
        public async Task Test_1G_File_TotalLines()
        {
            using var fileStream = new FileStream(OneGigFileName, FileMode.Open, FileAccess.Read);
            var iterator = CreateIterator(fileStream);

            try
            {
                var counter = 0L;
                do
                {
                    var hasNext = await iterator.MoveNextAsync();
                    if (!hasNext) break;
                    ++counter;
                }
                while (true);

                counter.ShouldBe(39841057L);
            }
            finally
            {
                await iterator.DisposeAsync();
            }
        }

        [TestMethod]
        public async Task Test_M100_Runs_Total_Lines()
        {
            var runs = new Dictionary<string, int>()
            {
                { "0.tmp", 768363 },
                { "1.tmp", 767614 },
                { "2.tmp", 767811 },
                { "3.tmp", 767419 },
                { "4.tmp", 767565 },
                { "5.tmp", 3475 }
            };


            foreach (var pair in runs)
            {
                var fileName = Path.Combine(@"f:\atesttask\M100_006.chunks", pair.Key);

                using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                var iterator = CreateIterator(fileStream);

                try
                {
                    var counter = 0L;
                    do
                    {
                        var hasNext = await iterator.MoveNextAsync();
                        if (!hasNext) break;
                        ++counter;
                    }
                    while (true);

                    counter.ShouldBe(pair.Value);
                }
                finally
                {
                    await iterator.DisposeAsync();
                }
            }
        }

        [TestMethod]
        public async Task Test_Read_And_Sort_M100_Runs()
        {
            var fileM100 = @"f:\atesttask\M100.txt";


            var singleRun = new FileInfo(fileM100);

            using var fileStream = new FileStream(singleRun.FullName, FileMode.Open, FileAccess.Read);
            var iterator = CreateIterator(fileStream);

            var records = new List<Record>(1000000);

            try
            {
                var counter = 0L;
                do
                {
                    var hasNext = await iterator.MoveNextAsync();
                    if (!hasNext) break;
                    records.Add(iterator.Current);
                    ++counter;
                }
                while (true);


                var sw = Stopwatch.StartNew();
                records.Sort(RecordComparer.Compare);
                sw.Stop();

                TestUtils.Logger($"Sorted {singleRun.FullName}, {singleRun.Length} bytes, {records.Count} strings in {sw.Elapsed.TotalSeconds:F3}");
            }
            finally
            {
                await iterator.DisposeAsync();
            }
        }

        [TestMethod]
        public async Task Test_Merge_01G_10_Runs()
        {
            var runsFolder = @"f:\atesttask\G001.chunks";
            await MergeRuns(runsFolder);
        }

        [TestMethod]
        public async Task Test_Merge_01G_25_Runs()
        {
            var runsFolder = @"f:\atesttask\G001_025.chunks";
            await MergeRuns(runsFolder);
        }

        [TestMethod]
        public async Task Test_Merge_08G_440_Runs()
        {
            var runsFolder = @"f:\atesttask\G008_440.chunks";
            await MergeRuns(runsFolder);
        }

        [TestMethod]
        public async Task Test_Merge_10G_100_Chunks()
        {
            var runsFolder = @"f:\atesttask\G010_100.chunks";
            await MergeRuns(runsFolder);
        }

        private static async Task MergeRuns(string runsFolder)
        {
            var outputFile = "Test_Merged_Runs.txt";

            var directoryFiles = new DirectoryInfo(runsFolder).GetFiles();

            var streams = directoryFiles.Select(file =>
            {
                var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);

                return (fileStream, new FileLineIterator(
                    fileStream,
                    maxBytesToConsume: long.MaxValue
                ));
            });


            var targetFileName = Path.Combine(runsFolder, "..", outputFile);
            File.Delete(targetFileName);

            await StreamMerger.Merge(streams.Select(s => s.Item2), targetFileName, TestUtils.Logger);

            foreach (var stream in streams)
            {
                stream.Item2.Dispose();
                stream.Item1.Dispose();
            }
        }
    }
}
