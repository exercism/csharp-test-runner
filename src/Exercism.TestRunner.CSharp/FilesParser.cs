using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Exercism.TestRunner.CSharp
{
    internal static class FilesParser
    {
        public static Files Parse(Options options)
        {
            var configuration = JsonSerializer.Deserialize<Configuration>(ConfigJson(options));
            return configuration!.Files;
        }

        private static string ConfigJson(Options options) =>
            File.ReadAllText(options.ConfigJsonPath());

        private static string ConfigJsonPath(this Options options) =>
            Path.Combine(options.InputDirectory, ".meta", "config.json");
    }
    
    internal class Files
    {
        [JsonPropertyName("solution")]
        public string[] Solution { get; set; }

        [JsonPropertyName("test")]
        public string[] Test { get; set; }
    }

    internal class Configuration
    {
        [JsonPropertyName("files")]
        public Files Files { get; set; }
    }
}