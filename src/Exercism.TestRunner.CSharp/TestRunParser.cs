using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunParser
    {
        public static TestRun Parse(Options options)
        {
            var logLines = File.ReadLines(options.BuildLogFilePath);
            var buildFailed = logLines.Any();

            if (buildFailed)
            {
                return TestRunWithError(logLines);
            }

            return TestRunWithoutError(options);
        }

        private static TestRun TestRunWithoutError(Options options)
        {
            var testResults = TestResultParser.FromFile(options.TestResultsFilePath);
            var status = testResults.Any(x => x.Status == TestStatus.Error) ? TestStatus.Error : TestStatus.Pass;

            return new TestRun
            {
                Status = status,
                Tests = testResults
            };
        }

        private static TestRun TestRunWithError(IEnumerable<string> logLines) =>
            new TestRun
            {
                Message = string.Join("\n", logLines),
                Status = TestStatus.Error
            };
    }
}