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

        private static void WriteMessage(this Utf8JsonWriter jsonTextWriter, string message)
        {
            if (message == null)
                return;

            jsonTextWriter.WriteString("message", message);
        }

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
            jsonTextWriter.WriteEndObject();
        }

        private static void WriteName(this Utf8JsonWriter jsonTextWriter, string name)
        {
            if (name == null)
                return;

            jsonTextWriter.WriteString("name", name);
        }
    }
}