using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    internal static class NormalizationExtensions
    {
        public static string NormalizeJson(this string json) =>
            JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.None, CreateJsonSerializerSettings()).NormalizeNewlines();

        public static string NormalizeNewlines(this string json) =>
            json.Replace("\n", Environment.NewLine);

        private static JsonSerializerSettings CreateJsonSerializerSettings() =>
            new JsonSerializerSettings { ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() } };
    }
}