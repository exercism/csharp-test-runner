using System;
using System.IO;
using System.Linq;
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
            new[]
            {
                Path.Combine(options.InputDirectory, ".meta", "config.json"),
                Path.Combine(options.InputDirectory, ".exercism", "config.json")
            }.First(File.Exists);
    }
    
    internal class Files
    {
        [JsonPropertyName("solution")]
        public string[] Solution { get; set; } = Array.Empty<string>();

        [JsonPropertyName("test")]
        public string[] Test { get; set; } = Array.Empty<string>();

        [JsonPropertyName("editor")]
        public string[] Editor { get; set; } = Array.Empty<string>();
    }

    internal class Configuration
    {
        [JsonPropertyName("files")]
        public Files Files { get; set; }
    }
}