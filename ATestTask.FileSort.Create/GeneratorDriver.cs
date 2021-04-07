using ATestTask.FileSort.Create.Streams;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ATestTask.FileSort.Create
{
    public static class GeneratorDriver
    {
        public static async Task Generate(string targetFilePath, long targetFileSizeInBytes, Action<string> log)
        {
            var generator = new TestDataGenerator(
                targetFilePath, 
                targetFileSizeInBytes,
                maxGeneratorsConcurrency: Environment.ProcessorCount,
                chunkStreamFactory: () => CreateChunkStream(),
                outputFileBufferLength: 512 * 1024,
                maxChunkSizeInBytes: 128 * 1024,
                logger: log);

            var perfReport = await generator.Generate();

            LogWritingSpeed(log, perfReport);
        }

        private static void LogWritingSpeed(Action<string> log, PerformanceReport perfReport)
        {
            double sizeInMiB = perfReport.RealSize / Units.BytesPerMiB;
            var totalSeconds = perfReport.TimeTaken.TotalSeconds;

            if (totalSeconds < 1)
            {
                log($"Generated {sizeInMiB} in under 1 second (lightning fast)!");
            }
            else
            {
                var speedInMiBPS = sizeInMiB / totalSeconds;
                log($"Generated {sizeInMiB} MiB in {totalSeconds:F3} seconds at {speedInMiBPS:F3} MiBPS.");
            }
        }

        private static LineStream CreateLineStream()
        {
            var _wordGenerators = new Func<string>[] {
                EnglishWords.First(EnglishWords.WordCount),
                EnglishWords.Last(811),
                EnglishWords.Subset(37, 613)
            };

            Action<StringBuilder> _numbersStream = new IncreasingNumberStream().NextNumber;

            return new LineStream(_numbersStream, _wordGenerators);
        }

        private static ChunkStream CreateChunkStream() => new (CreateLineStream(), new ASCIIEncoding());
    }
}
