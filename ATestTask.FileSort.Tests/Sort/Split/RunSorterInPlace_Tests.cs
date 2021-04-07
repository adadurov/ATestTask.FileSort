using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;
using ATestTask.FileSort.Sort.Split;
using System.Linq;
using Shouldly;
using System;

namespace ATestTask.FileSort.Tests.Sort.Split
{
    [TestClass]
    public class RunSorterInPlace_Tests
    {
        private readonly Action<string> _logger = TestUtils.Logger;

        [TestMethod]
        public async Task Test_M099_Read_Size_Should_Match()
        {
            var inputFile = @"f:\atesttask\M099.txt";

            var fileInfo = new FileInfo(inputFile);

            var splitHelper = new FileSplitHelper(fileInfo, _logger);
            var runs = splitHelper.GetRuns(runSizeHint: 100 * 1024 * 1024);

            var (offset, length) = runs.First();

            var inPlaceSorter = new RunSorterInPlace(fileInfo, offset, length, lineLengthHintInBytes: 24);

            var items = await inPlaceSorter.Read();

            var totalSizeOfItems = items.Records.Aggregate(0L, (acc, item) => acc + item.length);

            totalSizeOfItems.ShouldBe(new FileInfo(inputFile).Length);
        }

        [TestMethod]
        public async Task Test_M099_Write_Size_Should_Match()
        {
            var inputFile = @"f:\atesttask\M099.txt";

            var fileInfo = new FileInfo(inputFile);

            var splitHelper = new FileSplitHelper(fileInfo, _logger);
            var runs = splitHelper.GetRuns(runSizeHint: 100 * 1024 * 1024);

            var (offset, length) = runs.First();

            var inPlaceSorter = new RunSorterInPlace(fileInfo, offset, length, lineLengthHintInBytes: 24);

            var items = await inPlaceSorter.Read();

            // confirm size after read
            var totalSizeOfItems = items.Records.Aggregate(0L, (acc, item) => acc + item.length);
            totalSizeOfItems.ShouldBe(new FileInfo(inputFile).Length);

            var outputFile = "Test_Merged_Runs.txt";
            var path = Path.GetDirectoryName(inputFile);

            var targetFileName = Path.Combine(path, outputFile);
            File.Delete(targetFileName);

            inPlaceSorter.Sort(items);
            // confirm size after sort
            var totalSizeOfItemsAfterSort = items.Records.Aggregate(0L, (acc, item) => acc + item.length);
            totalSizeOfItemsAfterSort.ShouldBe(new FileInfo(inputFile).Length);

            await inPlaceSorter.Save(items, targetFileName, returnBuffer: true);
            items.Buffer = null;

            // confirm size after writing
            totalSizeOfItemsAfterSort.ShouldBe(new FileInfo(targetFileName).Length);
        }

        [TestMethod]
        public async Task Test_FileInPlaceSorter_G001_Run_Read()
        {
            var fileG001 = @"f:\atesttask\G001.txt";

            var fileInfo = new FileInfo(fileG001);

            var splitHelper = new FileSplitHelper(fileInfo, _logger);
            var runs = splitHelper.GetRuns(runSizeHint: 100 * 1024 * 1024);

            var (offset, length) = runs.First();

            var inPlaceSorter = new RunSorterInPlace(fileInfo, offset, length, lineLengthHintInBytes: 24);

            var outputFile = "Test_Merged_Runs.txt";
            var path = Path.GetDirectoryName(fileG001);

            var targetFileName = Path.Combine(path, outputFile);
            File.Delete(targetFileName);


            await inPlaceSorter.Read();
        }

        [TestMethod]
        public async Task Test_FileInPlaceSorter_G001_Run_Sort()
        {
            var fileG001 = @"f:\atesttask\G001.txt";

            var fileInfo = new FileInfo(fileG001);

            var splitHelper = new FileSplitHelper(fileInfo, _logger);
            var allRunsInfo = splitHelper.GetRuns(runSizeHint: 100 * 1024 * 1024);

            var (offset, length) = allRunsInfo.First();

            var inPlaceSorter = new RunSorterInPlace(fileInfo, offset, length, lineLengthHintInBytes: 24);


            var outputFile = "Test_Merged_Runs.txt";
            var path = Path.GetDirectoryName(fileG001);

            var targetFileName = Path.Combine(path, outputFile);
            File.Delete(targetFileName);


            var run = await inPlaceSorter.Read();

            inPlaceSorter.Sort(run);
        }


        [TestMethod]
        public async Task Test_FileInPlaceSorter_G001_Run_Save()
        {
            var fileG001 = @"f:\atesttask\G001.txt";

            var fileInfo = new FileInfo(fileG001);

            var splitHelper = new FileSplitHelper(fileInfo, _logger);
            var allRunsInfo = splitHelper.GetRuns(runSizeHint: 100 * 1024 * 1024);

            var (offset, length) = allRunsInfo.First();

            var inPlaceSorter = new RunSorterInPlace(fileInfo, offset, length, lineLengthHintInBytes: 24);


            var outputFile = "Test_Merged_Runs.txt";
            var path = Path.GetDirectoryName(fileG001);

            var targetFileName = Path.Combine(path, outputFile);
            File.Delete(targetFileName);


            var run = await inPlaceSorter.Read();

            inPlaceSorter.Sort(run);

            await inPlaceSorter.Save(run, targetFileName, returnBuffer: true);
        }

    }
}