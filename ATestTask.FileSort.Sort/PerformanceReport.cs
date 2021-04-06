using System;

namespace AltTestTask.FileSort.Sort
{
    class PerformanceReport
    {
        public TimeSpan TotalTime => SplitTime + MergeTime;
        public TimeSpan SplitTime { get; internal set; }
        public TimeSpan MergeTime { get; internal set; }
    }
}
