using AltTestTask.FileSort.Sort.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.IO;
using System.Threading.Tasks;

namespace AltTestTask.FileSort.Tests.Sort.Merge
{
    [TestClass]
    public class StreamMerger_Tests
    {
        [TestMethod]
        public async Task Test_Read_And_Sort_M100_Runs()
        {
            var testFile = @"f:\atesttask\M099.txt";

            var testFile_Sorted = testFile + ".sorted_test";

            using var fileStream = new FileStream(testFile, FileMode.Open, FileAccess.Read);
            var iterator = CreateIterator(fileStream);

            await StreamMerger.Merge(
                runs: new[] { iterator }, 
                targetFileName: testFile_Sorted,
                logger: TestUtils.Logger
                );

            var sourceFileInfo = new FileInfo(testFile);
            var targetFileInfo = new FileInfo(testFile_Sorted);

            targetFileInfo.Length.ShouldBe(sourceFileInfo.Length);

            File.Delete(testFile_Sorted);
        }

        private static FileLineIterator CreateIterator(FileStream fileStream)
        {
            return new FileLineIterator(
                fileStream,
                maxBytesToConsume: long.MaxValue
            );
        }

    }
}
