using CommandLine;

namespace AltTestTask.FileSort.Sort.CLI
{
    public class Options
    {
        [Value(0, Required = true, MetaName = "Name of the file to sort")]
        public string SourceFilePath { get; private set; }

        [Value(1, 
            Required = false, 
            MetaName = "Maximum number of parallel workers used to split file into sorted runs. \n" + 
                       "Default: 0 (use all CPU cores).")]
        public int MaxDegreeOfParallelism { get; private set; } = 0;

        [Value(2, Required = false, 
            MetaName = "The target size of a run (in MiB). The RAM consumption for sorting each run is ~X2.5 the size of the run")]
        public int RunSizeHintInMiB { get; internal set; } = 100;
    }
}