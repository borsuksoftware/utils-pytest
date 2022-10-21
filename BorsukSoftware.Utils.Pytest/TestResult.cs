using System;
using System.Collections.Generic;

namespace BorsukSoftware.Utils.Pytest
{
    /// <summary>
    /// Class representing the result of a single test
    /// </summary>
    public class TestResult
    {
        public bool Passed { get; }
        public string TestName { get; }
        public IReadOnlyList<string> Body { get; }
        public IReadOnlyList<string> StdOutLines { get; }
        public IReadOnlyList<string> LogMessages { get; }

        public TestResult(string testName, bool passed,
            IReadOnlyList<string> body,
            IReadOnlyList<string> stdOutLines,
            IReadOnlyList<string> logMessages)
        {
            if (string.IsNullOrEmpty(testName))
                throw new ArgumentNullException(nameof(testName));

            Passed = passed;
            TestName = testName;
            Body = body ?? Array.Empty<string>();
            StdOutLines = stdOutLines ?? Array.Empty<string>();
            LogMessages = logMessages ?? Array.Empty<string>();
        }
    }
}
