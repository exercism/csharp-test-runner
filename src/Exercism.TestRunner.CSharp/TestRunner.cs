using System;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.Extension.Exercism.TestLogger;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunner
    {
        public static void Run(Options options)
        {
            var testRunnerOutput = RunDotnetTestWithExercismLogger(options);

            var resultsFilePath = GetResultsFilePath(options);
            if (File.Exists(resultsFilePath))
                return;

            var testRun = new TestRun(testRunnerOutput.Normalized, TestStatus.Error, Array.Empty<TestResult>());
            TestRunWriter.WriteToFile(resultsFilePath, testRun);
        }

        private static TestRunnerOutput RunDotnetTestWithExercismLogger(Options options) =>
            new TestRunnerOutput(ProcessRunner.Run("dotnet", GetDotnetTestArguments(options)));

        private static string GetDotnetTestArguments(Options options) =>
            $"test {options.InputDirectory} --logger:exercism;ResultsDirectoryName={options.OutputDirectory} --test-adapter-path:.";

        private static string GetResultsFilePath(Options options) =>
            Path.GetFullPath(Path.Combine(options.OutputDirectory, "results.json"));
    }
}