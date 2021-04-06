using CommandLine;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AltTestTask.FileSort.Create.CLI
{
    public class Options
    {
        private int _sizeInMiB;

        private long _sizeInBytes;

        [Value(0, Required = true, MetaName = "Name of the target file, string")]
        public string TargetFilePath { get; private set; }

        [Value(1, Required = true, MetaName = "Size of the target file, MiB")]
        public int TargetFileSizeInMiB { get => _sizeInMiB; private set { _sizeInMiB = value; _sizeInBytes = value * Units.BytesPerMiB; } }

        public long TargetFileSizeInBytes => _sizeInBytes;
    }

    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var task = Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                  async (Options opts) => await RunCreateAndReturnExitCode(opts),
                  errors => Task.FromResult(ExitCodes.BadParameters));

            return await task;
        }

        private static async Task<int> RunCreateAndReturnExitCode(Options options)
        {
            try
            {
                ValidateOptionsOrThrow(options);

                await GeneratorDriver.Generate(
                    options.TargetFilePath,
                    options.TargetFileSizeInBytes,
                    Log);
            }
            catch (Exception ex)
            {
                WriteErrorMessage(ex);
                return ExitCodes.RuntimeError;
            }
            return ExitCodes.OK;
        }

        private static void ValidateOptionsOrThrow(Options options)
        {
            if (File.Exists(options.TargetFilePath))
            {
                throw new Exception($"File {options.TargetFilePath} already exists.");
            }

            if (options.TargetFileSizeInMiB < 1)
            {
                throw new Exception("Parameter <SizeInMiB>. Expected a positive integer less than 2**32-1");
            }
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
