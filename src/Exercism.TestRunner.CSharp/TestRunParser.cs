﻿using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp;

internal static class TestRunParser
{
    public static TestRun Parse(Options options, SyntaxTree testsSyntaxTree)
    {
        var logLines = File.ReadAllLines(options.BuildLogFilePath);
        var buildFailed = logLines.Length > 0;

        return buildFailed 
            ? TestRunWithError(logLines)
            : TestRunWithoutError(options, testsSyntaxTree);
    }

    private static TestRun TestRunWithoutError(Options options, SyntaxTree testsSyntaxTree)
    {
        var testResults = TestResultParser.FromFile(options.TestResultsFilePath, testsSyntaxTree);

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
        new()
        {
            Message = string.Join("\n", logLines.Select(NormalizeLogLine)),
            Status = TestStatus.Error,
            Tests = []
        };

    private static string NormalizeLogLine(this string logLine) =>
        logLine.RemoveProjectReference().RemovePath().UseUnixNewlines().Trim();

    private static string RemoveProjectReference(this string logLine)
    {
        var bracketIndex = logLine.LastIndexOf('[');
        return bracketIndex == -1 ? logLine : logLine[..(bracketIndex - 1)];
    }

    private static string RemovePath(this string logLine)
    {
        var testFileIndex = logLine.IndexOf(".cs(", StringComparison.Ordinal);
        if (testFileIndex == -1)
            return logLine;

        var lastDirectorySeparatorIndex = logLine.LastIndexOf(Path.DirectorySeparatorChar, testFileIndex);
        if (lastDirectorySeparatorIndex == -1)
            return logLine;

        return logLine[(lastDirectorySeparatorIndex + 1)..];
    }
}