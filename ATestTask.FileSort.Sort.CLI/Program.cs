using CommandLine;
using System;
using System.Threading.Tasks;

namespace ATestTask.FileSort.Sort.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var task = Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                  async (Options opts) => await RunSortAndReturnExitCode(opts),
                  errors => Task.FromResult(ExitCodes.BadParameters));

            return await task;
        }

        private static async Task<int> RunSortAndReturnExitCode(Options options)
        {
            try
            {
                var driver = SortDriver.Create(
                    options.SourceFilePath,
                    options.RunSizeHintInMiB,
                    options.MaxDegreeOfParallelism,
                    mergeBufferSizePerRun: 1024 * 1024,
                    Log);

                await driver.Sort();
            }
            catch (Exception ex)
            {
                WriteErrorMessage(ex);
                return ExitCodes.RuntimeError;
            }
            return 0;
        }

        private static void WriteErrorMessage(Exception ex)
        {
            Log(string.Join(Environment.NewLine, "Error: ", ex.Message));
        }

        private static void Log(string msg)
        {
            Console.WriteLine(string.Concat(DateTime.Now, ": ", msg));
        }

    }
}
