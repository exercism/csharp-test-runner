using System;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunParser
    {
        public static TestRun TestRunWithoutError(TestResult[] testResults) =>
            new()
            {
                Status = testResults.ToTestStatus(),
                Tests = testResults
            };

        private static TestStatus ToTestStatus(this TestResult[] tests)
        {
            if (tests.Any(test => test.Status == TestStatus.Fail))
                return TestStatus.Fail;

            if (tests.All(test => test.Status == TestStatus.Pass) && tests.Any())
                return TestStatus.Pass;

            return TestStatus.Error;
        }

        public static TestRun TestRunWithError(Diagnostic[] logLines) =>
            new()
            {
                Message = string.Join("\n", logLines.Select(NormalizeLogLine)),
                Status = TestStatus.Error,
                Tests = Array.Empty<TestResult>()
            };

        private static string NormalizeLogLine(this Diagnostic diagnostic) =>
            diagnostic.ToString().RemovePath(diagnostic).UseUnixNewlines().Trim();

        private static string RemovePath(this string logLine, Diagnostic diagnostic) =>
            diagnostic.Location == Location.None
                ? logLine
                : logLine.Replace(diagnostic.Location.SourceTree!.FilePath,
                Path.GetFileName(diagnostic.Location.SourceTree.FilePath));
    }
}