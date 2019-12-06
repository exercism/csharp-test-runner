using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using CommandLine;
using Humanizer;
using Serilog;

namespace Exercism.TestRunner.CSharp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Logging.Configure();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(CreateTestResults);
        }

        private static void CreateTestResults(Options options)
        {
            Log.Information("Running test runner for {Exercise} solution in directory {Directory}", options.Slug, options.InputDirectory);

            var logResultsFilePath = Path.Join(options.OutputDirectory, "results.trx");
            var resultsFilePath = Path.Join(options.OutputDirectory, "results.json");
            
            

//            var processStartInfo = new ProcessStartInfo("dotnet", $"test {options.InputDirectory} --logger:\"trx;LogFileName={logResultsFilePath}\"");
//            processStartInfo.RedirectStandardOutput = true;
//            var process = Process.Start(processStartInfo);
//            process.WaitForExit();

            
            
//            var xDocument = XDocument.Load(File.OpenRead(logResultsFilePath));

//
//            var xmlSerializer = new XmlSerializer(typeof(XmlTestRun));
//            var testRun = (XmlTestRun)xmlSerializer.Deserialize(File.OpenRead(logResultsFilePath));
//
//            var testResults = testRun.Results.UnitTestResult.Select(x => new TestResult { Status = ParseStatus(x.Outcome), Name = x.TestName, Message = x.Output.ErrorInfo.Message }).ToArray();
//            var status = testResults.All(t => t.Status == TestStatus.Pass)
//                ? TestStatus.Pass
//                : testResults.Any(t => t.Status == TestStatus.Fail)
//                    ? TestStatus.Fail
//                    : TestStatus.Error;
//            var testsResult = new TestRun { Tests = testResults, Status = status };
//
//            var jsonSerializerOptions = new JsonSerializerOptions() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
//            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
//            var serialize = JsonSerializer.Serialize(testResults, jsonSerializerOptions);
//            
//            File.WriteAllText(resultsFilePath, serialize);

            var name = options.Slug.Dehumanize().Pascalize();

            Log.Information("Ran test runner for {Exercise} solution in directory {Directory}", options.Slug, options.OutputDirectory);
        }

        private static TestStatus ParseStatus(string argOutcome) =>
            argOutcome switch
            {
                "Failed" => TestStatus.Fail,
                "Passed" => TestStatus.Pass,
                _ => TestStatus.Error
            };
    }
}