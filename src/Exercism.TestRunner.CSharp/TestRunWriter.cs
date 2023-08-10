using System.IO;
using System.Text.Json;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunWriter
    {
        private static readonly JsonWriterOptions JsonWriterOptions = new() {Indented = true};

        public static void WriteToFile(this TestRun testRun, Options options)
        {
            using var fileStream = File.Create(options.ResultsJsonPath());
            using var jsonWriter = new Utf8JsonWriter(fileStream, JsonWriterOptions);
            jsonWriter.WriteStartObject();
            jsonWriter.WriteNumber("version", testRun.Version);
            jsonWriter.WriteString("status", testRun.Status.ToString().ToLower());
            jsonWriter.WriteStringIfNotEmpty("message", testRun.Message);
            jsonWriter.WriteTestResults(testRun.Tests);
            jsonWriter.WriteEndObject();
            jsonWriter.Flush();
            fileStream.WriteByte((byte)'\n');
        }
        
        private static void WriteTestResults(this Utf8JsonWriter jsonWriter, TestResult[] testResults)
        {
            if (testResults.Length == 0)
                return;
            
            jsonWriter.WriteStartArray("tests");
            
            foreach (var testResult in testResults)
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("name", testResult.Name);
                jsonWriter.WriteString("status", testResult.Status.ToString().ToLower());
                if (testResult.TaskId.HasValue)
                    jsonWriter.WriteNumber("task_id", testResult.TaskId.Value);
                jsonWriter.WriteStringIfNotEmpty("message", testResult.Message);
                jsonWriter.WriteStringIfNotEmpty("output", testResult.Output);
                jsonWriter.WriteStringIfNotEmpty("test_code", testResult.TestCode);
                jsonWriter.WriteEndObject();
            }
            
            jsonWriter.WriteEndArray();
        }

        private static void WriteStringIfNotEmpty(this Utf8JsonWriter jsonWriter, string property, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            
            jsonWriter.WriteString(property, value);
        }


        private static string ResultsJsonPath(this Options options) =>
            Path.GetFullPath(Path.Combine(options.OutputDirectory, "results.json"));
    }
}