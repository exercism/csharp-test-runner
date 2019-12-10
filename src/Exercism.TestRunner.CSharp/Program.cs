using System.Threading.Tasks;
using CommandLine;
using Serilog;

namespace Exercism.TestRunner.CSharp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Logging.Configure();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => CreateTestResults(options).GetAwaiter().GetResult());
        }

        private static async Task CreateTestResults(Options options)
        {
            Log.Information("Running test runner for {Exercise} solution in directory {Directory}", options.Slug, options.InputDirectory);

            var testRun = await TestRunner.Run(options);
            TestRunWriter.WriteToFile(options, testRun);

            Log.Information("Ran test runner for {Exercise} solution in directory {Directory}", options.Slug, options.OutputDirectory);
        }
    }
}