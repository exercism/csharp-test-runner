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
            if (testRunnerOutput.Success)
                return;

            var testRun = new TestRun(testRunnerOutput.Normalized, TestStatus.Error, Array.Empty<TestResult>());

            var resultsFilePath = GetResultsFilePath(options);
            TestRunWriter.WriteToFile(resultsFilePath, testRun);
        }

        private static TestRunnerOutput RunDotnetTestWithExercismLogger(Options options)
        {
            var (output, exitCode) = ProcessRunner.Run("dotnet", GetDotnetTestArguments(options));
            return new TestRunnerOutput(output, exitCode == 0);
        }

        private static string GetDotnetTestArguments(Options options) =>
            $"test {options.InputDirectory} --logger:exercism;ResultsDirectoryName={options.OutputDirectory} --test-adapter-path:.";

        private static string GetResultsFilePath(Options options) =>
            Path.GetFullPath(Path.Combine(options.OutputDirectory, "results.json"));
    }
}