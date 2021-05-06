using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    internal static class TestRunResultReader
    {
        public static string ReadActual(TestSolution solution) =>
            ReadTestRunResult(solution, "results.json");

        public static string ReadExpected(TestSolution solution) =>
            ReadTestRunResult(solution, "expected_results.json");

        private static string ReadTestRunResult(TestSolution solution, string fileName)
        {
            var testRunResult = DeserializeTestRunResult(solution, fileName);
            NormalizeTestRunResult(testRunResult);

            return JsonSerializer.Serialize(testRunResult, CreateJsonSerializerOptions());
        }

        private static TestRunResult DeserializeTestRunResult(TestSolution solution, string fileName) =>
            JsonSerializer.Deserialize<TestRunResult>(ReadFile(solution, fileName), CreateJsonSerializerOptions());

        private static string ReadFile(TestSolution solution, string fileName) =>
            File.ReadAllText(Path.Combine(solution.DirectoryFullPath, fileName));

        private static void NormalizeTestRunResult(TestRunResult testRunResult)
        {
            static int Comparison(TestResult x, TestResult y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            Array.Sort(testRunResult.Tests, Comparison);
        }

        private static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = null
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        private enum TestStatus
        {
            Pass,
            Fail,
            Error
        }

        private struct TestRunResult
        {
            [JsonPropertyName("version")]
            public int Version { get; set; }
            
            [JsonPropertyName("status")]
            public TestStatus Status { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("tests")]
            public TestResult[] Tests { get; set; }
        }

        private struct TestResult
        {
            [JsonPropertyName("status")]
            public TestStatus Status { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }
        
            [JsonPropertyName("task_id")]
            public int? TaskId { get; set; }

            [JsonPropertyName("output")]
            public string Output { get; set; }
        }
    }
}