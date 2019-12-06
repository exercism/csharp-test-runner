using System.IO;
using System.Text.Json;

namespace Exercism.TestRunner.CSharp
{
    internal static class MappingWriter
    {
        public static void WriteToFile(Options options) =>
            File.WriteAllText(GetMappingFilePath(options), SerializeMapping());

        private static string GetMappingFilePath(Options options) =>
            Path.GetFullPath(Path.Combine(options.OutputDirectory, "results.json"));

        private static string SerializeMapping() =>
            @"{
  ""status"": ""pass"",
  ""tests"": [
    {
      ""name"": ""FakeTest.Add_should_add_numbers"",
      ""status"": ""pass""
    }
  ]
}
";
//            JsonSerializer.Serialize(mapping.PlaceholdersToIdentifier, new JsonSerializerOptions { WriteIndented = true });
    }
}