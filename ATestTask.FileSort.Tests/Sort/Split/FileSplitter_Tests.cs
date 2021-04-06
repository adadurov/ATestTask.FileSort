using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;
using AltTestTask.FileSort.Sort.Split;
using AltTestTask.FileSort.Sort;
using System.Diagnostics;
using System.Linq;
using Shouldly;

namespace AltTestTask.FileSort.Tests.Sort.Split
{
    [TestClass]
    public class FileSplitter_Tests
    {
        private FileSplitter _fileSplitter;

        [TestInitialize]
        public void Setup()
        {
            _fileSplitter = new FileSplitter(TestUtils.Logger);
        }

        [TestMethod]
        public async Task Test_FileSplitter_M_099_file()
        {
            var inputFile = @"f:\atesttask\M099.txt";

            var tempDir = Path.Combine(Path.GetDirectoryName(inputFile), "Test_TempDirForRuns");

            using var repository = new ExternalRunRepository(tempDir);

            await _fileSplitter.Split(
                pathToSourceFile: inputFile,
                repository: repository,
                runSizeHintInBytes: 100 * 1024 * 1024,
                lineLengthHintInBytes: 24,
                maxDegreeOfParallelism: 4
                );

            var singleRun = new DirectoryInfo(tempDir).GetFiles().First();

            singleRun.Length.ShouldBe(new FileInfo(inputFile).Length);

            TestUtils.Logger("Done");
        }

        [TestMethod]
        public async Task Test_FileSplitter_G_001_file()
        {
            var fileG001 = @"f:\atesttask\G001.txt";

            var tempDir = Path.Combine(Path.GetDirectoryName(fileG001), "Test_TempDirForRuns");

            using var repository = new ExternalRunRepository(tempDir);

            await _fileSplitter.Split(
                pathToSourceFile: fileG001,
                repository: repository,
                runSizeHintInBytes: 100 * 1024 * 1024,
                lineLengthHintInBytes: 24,
                maxDegreeOfParallelism: 4
                );

            TestUtils.Logger("Done");
        }

        [TestMethod]
        public async Task Test_FileSplitter_G_010_file()
        {
            var inputFile = @"f:\atesttask\G010.txt";

            var tempDir = Path.Combine(Path.GetDirectoryName(inputFile), "Test_TempDirForRuns");

            using var repository = new ExternalRunRepository(tempDir);

            await _fileSplitter.Split(
                pathToSourceFile: inputFile,
                repository: repository,
                runSizeHintInBytes: 100 * 1024 * 1024,
                lineLengthHintInBytes: 24,
                maxDegreeOfParallelism: 4
                );

            TestUtils.Logger("Done");
        }

    }
}