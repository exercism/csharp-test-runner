using System.Threading.Tasks;
using CommandLine;
using Microsoft.Build.Locator;
using Serilog;

namespace Exercism.TestRunner.CSharp
{
    public static class Program
    {   
        public static void Main(string[] args)
        {   
            if (!MSBuildLocator.IsRegistered)
                MSBuildLocator.RegisterDefaults();
            
            Logging.Configure();

            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsedAsync(CreateTestResults);
        }

        private static async Task CreateTestResults(Options options)
        {
            Log.Information("Running test runner for {Exercise} solution in directory {Directory}", options.Slug, options.InputDirectory);

            var testRun = await TestRunner.RunTests(options);
            TestRunWriter.WriteToFile(testRun, options);

            Log.Information("Ran test runner for {Exercise} solution in directory {Directory}", options.Slug, options.OutputDirectory);
        }
    }
}