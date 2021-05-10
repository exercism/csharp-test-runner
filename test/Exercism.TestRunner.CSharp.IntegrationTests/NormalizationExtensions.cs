using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    internal static class NormalizationExtensions
    {
        public static string NormalizeJson(this string json) =>
            JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.None, CreateJsonSerializerSettings()).NormalizeNewlines();

        private static string NormalizeNewlines(this string json) =>
            json.Replace("\r\n", "\n");

        private static JsonSerializerSettings CreateJsonSerializerSettings() =>
            new() { ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() } };
    }
}