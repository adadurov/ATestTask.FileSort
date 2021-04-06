using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Shouldly;
using System.Linq;
using AltTestTask.FileSort.Sort.Split;

namespace AltTestTask.FileSort.Tests.Sort.Split
{
    [TestClass]
    public class FileSplitHelper_Tests
    {
        [TestMethod]
        public void Test_FileSplitterHelper_1G_file()
        {
            var fileG001 = @"f:\atesttask\G001.txt";

            var fileInfo = new FileInfo(fileG001);

            var splitHelper = new FileSplitHelper(fileInfo, TestUtils.Logger);

            var runs = splitHelper.GetRuns(runSizeHint: 100 * 1024 * 1024);

            runs.Count.ShouldBe(11);

            var sum = runs.Aggregate(0L, (sum, c) => sum + c.length);

            sum.ShouldBe(fileInfo.Length);

            var sizeMatchOffset = runs.Zip(runs.Skip(1)).Select(p => (p.First.offset, p.First.offset + p.First.length == p.Second.offset));

            foreach (var match in sizeMatchOffset)
            {
                match.Item2.ShouldBeTrue($"Run at offset {match.Item2} doesn't fit");
            }
        }
    }
}