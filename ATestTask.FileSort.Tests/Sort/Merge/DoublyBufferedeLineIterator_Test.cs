using ATestTask.FileSort.Sort.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ATestTask.FileSort.Tests.Sort.Merge
{
    [TestClass]
    public class DoublyBufferedLineIterator_Test
    {
        public string OneGigFileName => @"f:\atesttask\G001.txt";

        private static DoublyBufferedLineIterator CreateIterator(FileStream fileStream)
        {
            return new DoublyBufferedLineIterator(
                fileStream,
                1024 * 1024
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
        public async Task Test_Merge_01G_10_Runs_HDD()
        {
            var runsFolder = @"d:\atesttask\G001.chunks";
            await MergeRuns(runsFolder);
        }

        [TestMethod]
        public async Task Test_Merge_01G_25_Runs_HDD()
        {
            var runsFolder = @"d:\atesttask\G001_025.chunks";
            await MergeRuns(runsFolder);
        }

        [TestMethod]
        public async Task Test_Merge_10G_100_Runs()
        {
            var runsFolder = @"f:\atesttask\G010_100.chunks";
            await MergeRuns(runsFolder);
        }

        [TestMethod]
        public async Task Test_Merge_10G_100_Runs_HDD()
        {
            var runsFolder = @"d:\atesttask\G010_100.chunks";
            await MergeRuns(runsFolder);
        }

        private static async Task MergeRuns(string runsFolder)
        {
            var outputFile = "Test_Merged_Runs.txt";

            var directoryFiles = new DirectoryInfo(runsFolder).GetFiles();

            var streams = directoryFiles.Select(file =>
            {
                var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                var iterator = new DoublyBufferedLineIterator(fileStream, 1024 * 1024);

                return (fileStream, iterator);
            });


            var targetFileName = Path.Combine(runsFolder, "..", outputFile);
            File.Delete(targetFileName);

            await StreamMerger.Merge(streams.Select(s => s.Item2), targetFileName, TestUtils.Logger);

            foreach (var stream in streams)
            {
                stream.iterator.Dispose();
                stream.fileStream.Dispose();
            }
        }

    }
}
