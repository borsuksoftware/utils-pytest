using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.Utils.Pytest.OutputProcessor
{
    /// <summary>
    /// Class to process a log file from a pytest run and return the set of expected results
    /// </summary>
    /// <remarks>
    /// This processes the output for extra test summary info. Explicitly, this means that the tests should be run
    /// with -r flag specified alongside which tests we want summaries for. Our recommendation is for -rA (all).
    /// </remarks>
    public class Processor
    {
        public IReadOnlyCollection<TestResult> ProcessLogOutput(System.IO.TextReader textReader)
        {
            if (textReader == null)
                throw new ArgumentNullException(nameof(textReader));

            var introLinesPassesRegex = new System.Text.RegularExpressions.Regex("^(=)+ PASSES (=)+$");
            var introLinesFailuresRegex = new System.Text.RegularExpressions.Regex("^(=)+ FAILURES (=)+$");
            var testStartRegex = new System.Text.RegularExpressions.Regex("^(_)+ (?<testName>.*) (_)+$");

            var startCapturedStdoutCall = new System.Text.RegularExpressions.Regex("^-+ Captured stdout call -+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var startCapturedLogCall = new System.Text.RegularExpressions.Regex("^-+ Captured log call -+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var generatedJUnitFileRegex = new System.Text.RegularExpressions.Regex("^-+ generated xml file: .* -+$");
            var shortTestSummaryInfo = new System.Text.RegularExpressions.Regex("^=+ short test summary info =+$");


            var output = new List<TestResult>();

            string activeTestName = null;
            List<string> activeTestStdOutLines = null, activeTestLogLines = null, activeTestBodyLines = null;
            var state = ProcessorState.Intro;
            var testProcessingState = TestProcessingState.None;
            bool areTestsPassing = true;
            while (true)
            {
                var line = textReader.ReadLine();
                if (line == null)
                    break;

                switch (state)
                {
                    case ProcessorState.Intro:
                        if (introLinesPassesRegex.IsMatch(line))
                        {
                            state = ProcessorState.LookingForTest;
                            areTestsPassing = true;
                        }
                        else if (introLinesFailuresRegex.IsMatch(line))
                        {
                            state = ProcessorState.LookingForTest;
                            areTestsPassing = false;
                        }
                        break;

                    case ProcessorState.LookingForTest:
                        {
                            var testStartMatch = testStartRegex.Match(line);
                            if (testStartMatch.Success)
                            {
                                activeTestName = testStartMatch.Groups["testName"].Value;
                                activeTestBodyLines = new List<string>();
                                activeTestStdOutLines = new List<string>();
                                activeTestLogLines = new List<string>();
                                state = ProcessorState.ProcessingTest;
                                testProcessingState = TestProcessingState.None;
                            }
                        }
                        break;

                    case ProcessorState.ProcessingTest:
                        {
                            // Check if we've hit the end of all tests
                            if (generatedJUnitFileRegex.IsMatch(line) || shortTestSummaryInfo.IsMatch(line))
                            {
                                // Need to wrap up existing output...
                                output.Add(new TestResult(activeTestName, areTestsPassing, activeTestBodyLines, activeTestStdOutLines, activeTestLogLines));

                                state = ProcessorState.TestsFinished;
                            }

                            // Check if we're moving to the next set of tests
                            if (introLinesPassesRegex.IsMatch(line))
                            {
                                output.Add(new TestResult(activeTestName, areTestsPassing, activeTestBodyLines, activeTestStdOutLines, activeTestLogLines));
                                state = ProcessorState.LookingForTest;
                                areTestsPassing = true;
                            }
                            else if (introLinesFailuresRegex.IsMatch(line))
                            {
                                output.Add(new TestResult(activeTestName, areTestsPassing, activeTestBodyLines, activeTestStdOutLines, activeTestLogLines));
                                state = ProcessorState.LookingForTest;
                                areTestsPassing = false;
                            }

                            var testStartMatch = testStartRegex.Match(line);
                            if (testStartMatch.Success)
                            {
                                // Need to wrap up existing output...
                                output.Add(new TestResult(activeTestName, areTestsPassing, activeTestBodyLines, activeTestStdOutLines, activeTestLogLines));

                                activeTestName = testStartMatch.Groups["testName"].Value;
                                activeTestStdOutLines = new List<string>();
                                activeTestLogLines = new List<string>();
                                activeTestBodyLines = new List<string>();
                                state = ProcessorState.ProcessingTest;
                                testProcessingState = TestProcessingState.None;
                                continue;
                            }

                            if (startCapturedStdoutCall.IsMatch(line))
                            {
                                testProcessingState = TestProcessingState.StdOut;
                                continue;
                            }

                            if (startCapturedLogCall.IsMatch(line))
                            {
                                testProcessingState = TestProcessingState.Logs;
                                continue;
                            }

                            switch (testProcessingState)
                            {
                                case TestProcessingState.None:
                                    activeTestBodyLines.Add(line);
                                    break;

                                case TestProcessingState.Logs:
                                    activeTestLogLines.Add(line);
                                    break;

                                case TestProcessingState.StdOut:
                                    activeTestStdOutLines.Add(line);
                                    break;
                            }
                            break;
                        }
                }
            }

            return output;
        }


        #region ProcessorState enum

        private enum ProcessorState
        {
            Intro,
            TestSessionStarted,
            ProcessingTest,
            LookingForTest,
            TestsFinished,
            LookingForFailure,
            ProcessingFailure
        }

        # endregion

        private enum TestProcessingState
        {
            None,
            StdOut,
            Logs
        }
    }
}
