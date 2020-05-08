using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunWriter
    {
        public static void WriteToFile(Options options, TestRun testRun) =>
            File.WriteAllText(GetResultsFilePath(options), SerializeAsJson(testRun));

        private static string SerializeAsJson(TestRun testRun) =>
            JsonSerializer.Serialize(testRun.ToJsonTestRun(), CreateJsonSerializerOptions());

        private static JsonSerializerOptions CreateJsonSerializerOptions() =>
            new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = true
            };

        private static string GetResultsFilePath(Options options) =>
            Path.GetFullPath(Path.Combine(options.OutputDirectory, "results.json"));

        private static JsonTestResult ToJsonTestResult(this TestResult testResult) =>
            new JsonTestResult
            {
                Name = testResult.Name,
                Test = testResult.Test,
                Expected = testResult.Expected,
                Status = testResult.Status.ToString().ToLower(),
                Message = testResult.Message.ToNullIfEmptyOrWhiteSpace(),
                Output = testResult.Output.ToNullIfEmptyOrWhiteSpace()
            };

        private static JsonTestRun ToJsonTestRun(this TestRun testRun) =>
            new JsonTestRun
            {
                Status = testRun.Status.ToString().ToLower(),
                Message = testRun.Message.ToNullIfEmptyOrWhiteSpace(),
                Tests = testRun.Tests.Select(ToJsonTestResult).ToArray()
            };

        private static string ToNullIfEmptyOrWhiteSpace(this string str) =>
            string.IsNullOrWhiteSpace(str) ? null : str.Trim().Replace("\r\n", "\n");

        private class JsonTestResult
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("test")]
            public string Test { get; set; }

            [JsonPropertyName("expected")]
            public string Expected { get; set; }

            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("output")]
            public string Output { get; set; }
        }

        private class JsonTestRun
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("tests")]
            public JsonTestResult[] Tests { get; set; }
        }
    }
}