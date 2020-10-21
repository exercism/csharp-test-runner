using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunWriter
    {
        public static void WriteToFile(TestRun testRun, Options options) =>
            File.WriteAllText(GetResultsFilePath(options), SerializeAsJson(testRun));

        private static string SerializeAsJson(TestRun testRun) =>
            JsonSerializer.Serialize(testRun, CreateJsonSerializerOptions());

        private static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = true,
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            
            return options;
        }

        private static string GetResultsFilePath(Options options) =>
            Path.GetFullPath(Path.Combine(options.OutputDirectory, "results.json"));
    }
}