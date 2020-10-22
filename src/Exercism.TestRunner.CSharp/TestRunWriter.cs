using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunWriter
    {
        public static void WriteToFile(this TestRun testRun, string resultsJsonFilePath) =>
            File.WriteAllText(resultsJsonFilePath, testRun.ToJson());

        private static string ToJson(this TestRun testRun) =>
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
    }
}