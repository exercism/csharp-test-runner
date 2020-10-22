using System;
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

            return new TestRun
            {
                Status = testResults.ToTestStatus(),
                Tests = testResults
            };
        }
        
        private static TestStatus ToTestStatus(this TestResult[] tests)
        {
            if (tests.Any(test => test.Status == TestStatus.Fail))
                return TestStatus.Fail;

            if (tests.All(test => test.Status == TestStatus.Pass) && tests.Any())
                return TestStatus.Pass;

            return TestStatus.Error;
        }

        private static TestRun TestRunWithError(IEnumerable<string> logLines) =>
            new TestRun
            {
                Message = string.Join("\n", logLines.Select(NormalizeLogLine)),
                Status = TestStatus.Error,
                Tests = Array.Empty<TestResult>()
            };
            
        private static string NormalizeLogLine(this string logLine) =>
            logLine.RemoveProjectReference().UseUnixNewlines().Trim();
        
        private static string RemoveProjectReference(this string logLine) =>
            logLine[..(logLine.LastIndexOf('[') - 1)];
    }
}