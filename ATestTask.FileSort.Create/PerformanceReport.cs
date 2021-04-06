using System;

namespace AltTestTask.FileSort.Create
{
    internal class PerformanceReport
    {
        public long RealSize { get; internal set; }
        public TimeSpan TimeTaken { get; internal set; }
        public long WriteRequests { get; internal set; }
    }
}