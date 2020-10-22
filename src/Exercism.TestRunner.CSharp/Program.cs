using CommandLine;

using Serilog;

namespace Exercism.TestRunner.CSharp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Logging.Configure();

            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(CreateTestResults);
        }

        private static void CreateTestResults(Options options)
        {
            Log.Information("Running test runner for {Exercise} solution in directory {Directory}", options.Slug, options.InputDirectory);

            var testSuite = TestSuite.FromOptions(options);
            var testRun = testSuite.Run();
            testRun.WriteToFile(options.ResultsJsonFilePath);

            Log.Information("Ran test runner for {Exercise} solution in directory {Directory}", options.Slug, options.OutputDirectory);
        }
    }
}