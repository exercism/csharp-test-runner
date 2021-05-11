using System;

using CommandLine;

namespace Exercism.TestRunner.CSharp
{
    public static class Program
    {
        public static void Main(string[] args) =>
            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(CreateTestResults);

        private static void CreateTestResults(Options options)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:u}] Running test runner for '{options.Slug}' solution...");

            var testRun = TestSuite.RunTests(options);
            testRun.WriteToFile(options.ResultsJsonFilePath);

            Console.WriteLine($"[{DateTimeOffset.UtcNow:u}] Ran test runner for '{options.Slug}' solution");
        }
    }
}