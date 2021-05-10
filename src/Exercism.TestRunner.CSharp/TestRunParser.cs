using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunParser
    {
        public static TestRun Parse(Options options, Compilation compilation)
        {
            var errors = compilation.GetDiagnostics()
                .Where(diag => diag.Severity == DiagnosticSeverity.Error)
                .ToArray();

            if (errors.Any())
            {
                return TestRunWithError(errors);
            }

            return TestRunWithoutError(options, compilation);
        }

        private static TestRun TestRunWithoutError(Options options, Compilation compilation)
        {
            // var testResults = TestResultParser.FromFile(options.TestResultsFilePath, compilation);
            var testResults = Array.Empty<TestResult>();

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

        private static TestRun TestRunWithError(Diagnostic[] logLines) =>
            new TestRun
            {
                Message = string.Join("\n", logLines.Select(NormalizeLogLine)),
                Status = TestStatus.Error,
                Tests = Array.Empty<TestResult>()
            };

        private static string NormalizeLogLine(this Diagnostic diagnostic) =>
            diagnostic.GetMessage().RemoveProjectReference().RemovePath().UseUnixNewlines().Trim();

        private static string RemoveProjectReference(this string logLine) =>
            logLine[..(logLine.LastIndexOf('[') - 1)];

        private static string RemovePath(this string logLine)
        {
            var testFileIndex = logLine.IndexOf(".cs(", StringComparison.Ordinal);
            if (testFileIndex == -1)
                return logLine;

            var lastDirectorySeparatorIndex = logLine.LastIndexOf(Path.DirectorySeparatorChar, testFileIndex);
            if (lastDirectorySeparatorIndex == -1)
                return logLine;

            return logLine.Substring(lastDirectorySeparatorIndex + 1);
        }
    }
}