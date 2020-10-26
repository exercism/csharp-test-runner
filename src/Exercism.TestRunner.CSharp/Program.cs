using CommandLine;

using System;

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
            Console.WriteLine($"Running test runner for '{options.Slug}' solution...");

            var testSuite = TestSuite.FromOptions(options);
            var testRun = testSuite.Run();
            testRun.WriteToFile(options.ResultsJsonFilePath);

            Console.WriteLine($"Ran test runner for '{options.Slug}' solution");
        }
    }
}