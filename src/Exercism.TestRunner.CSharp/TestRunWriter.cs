using System.IO;
using System.Text.Json;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunWriter
    {
        public static void WriteToFile(Options options, TestRun solutionAnalysis)
        {
            var filePath = GetResultsFilePath(options);

            using var fileStream = File.Create(filePath);
            using var jsonTextWriter = new Utf8JsonWriter(fileStream, new JsonWriterOptions { Indented = true });
            jsonTextWriter.WriteStartObject();
            jsonTextWriter.WriteStatus(solutionAnalysis.Status);
            jsonTextWriter.WriteMessage(solutionAnalysis.Message);
            jsonTextWriter.WriteTests(solutionAnalysis.Tests);
            jsonTextWriter.WriteEndObject();
            jsonTextWriter.Flush();
        }

        private static string GetResultsFilePath(Options options) =>
            Path.GetFullPath(Path.Combine(options.OutputDirectory, "results.json"));

        private static void WriteMessage(this Utf8JsonWriter jsonTextWriter, string message) =>
            jsonTextWriter.WriteOptionalString("message", message);

        private static void WriteStatus(this Utf8JsonWriter jsonTextWriter, TestStatus status) =>
            jsonTextWriter.WriteString("status", status.ToString().ToLower());

        private static void WriteTests(this Utf8JsonWriter jsonTextWriter, TestResult[] tests)
        {
            jsonTextWriter.WritePropertyName("tests");
            jsonTextWriter.WriteStartArray();

            foreach (var test in tests)
                jsonTextWriter.WriteTest(test);

            jsonTextWriter.WriteEndArray();
        }

        private static void WriteTest(this Utf8JsonWriter jsonTextWriter, TestResult test)
        {
            jsonTextWriter.WriteStartObject();
            jsonTextWriter.WriteName(test.Name);
            jsonTextWriter.WriteStatus(test.Status);
            jsonTextWriter.WriteMessage(test.Message);
            jsonTextWriter.WriteOutput(test.Output);
            jsonTextWriter.WriteEndObject();
        }

        private static void WriteName(this Utf8JsonWriter jsonTextWriter, string name) =>
            jsonTextWriter.WriteOptionalString("name", name);

        private static void WriteOutput(this Utf8JsonWriter jsonTextWriter, string output) =>
            jsonTextWriter.WriteOptionalString("output", output);

        private static void WriteOptionalString(this Utf8JsonWriter jsonTextWriter, string propertyName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            jsonTextWriter.WriteString(propertyName, StringExtensions.Normalize(value));
        }
    }
}