using System;
using System.Diagnostics;

namespace ATestTask.FileSort.Tests
{
    public static class TestUtils
    {
        public static Action<string> Logger => s => Trace.WriteLine(s);
    }
}
