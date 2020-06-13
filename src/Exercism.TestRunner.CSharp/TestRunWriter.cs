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

        // private static string ToNullIfEmptyOrWhiteSpace(this string str) =>
        //     string.IsNullOrWhiteSpace(str) ? null : str.Trim().Replace("\r\n", "\n");

        
    }
    
    internal class TestResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public TestStatus Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("output")]
        public string Output { get; set; }
    }

    internal class TestRun
    {
        [JsonPropertyName("status")]
        public TestStatus Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("tests")]
        public TestResult[] Tests { get; set; }
    }
    
    internal enum TestStatus
    {
        Pass,
        Fail,
        Error
    }
}