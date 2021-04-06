using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AltTestTask.FileSort.Sort.Split
{
    public class FileSplitter
    {
        private readonly Action<string> _logger;

        public FileSplitter(Action<string> logger)
        {
            _logger = logger;
        }

        public async Task Split(
            string pathToSourceFile, 
            ExternalRunRepository repository, 
            int runSizeHintInBytes, 
            int lineLengthHintInBytes,
            int maxDegreeOfParallelism
            )
        {
            var inputFileInfo = new FileInfo(pathToSourceFile);

            var splitHelper = new FileSplitHelper(inputFileInfo, _logger);
            var runSplitInfo = splitHelper.GetRuns(runSizeHintInBytes);

            var workItems = Enumerable.Range(1, runSplitInfo.Count).Zip(runSplitInfo).Select(i =>
            {
                var ch = i.Second;
                var splitter = new RunSorterInPlace(inputFileInfo, ch.offset, ch.length, lineLengthHintInBytes);
                return new
                {
                    Number = i.First,
                    Run = ch,
                    Splitter = splitter,
                    TargetFile = repository.Rent()
                };
            });

            await workItems.AsyncParallelForEach(async workItem =>
            {
                var workItemName = $"{workItem.Number, 5}: [{workItem.Run.offset}, {workItem.Run.offset + workItem.Run.length})";

                _logger($"{workItemName,-40} reading");
                var run = await workItem.Splitter.Read();

                _logger($"{workItemName,-40}         sorting");
                workItem.Splitter.Sort(run);

                _logger($"{workItemName,-40}                 saving");
                await workItem.Splitter.Save(run, workItem.TargetFile, returnBuffer: true);
                
                run.Buffer = null;
                _logger($"{workItemName,-40}                         done");

                GC.Collect();
            },
            maxDegreeOfParallelism: maxDegreeOfParallelism);
        }
    }
}
