using System.Text.Json.Serialization;

namespace Exercism.TestRunner.CSharp
{
    internal enum TestStatus
    {
        Pass,
        Fail,
        Error
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

        [JsonPropertyName("test_code")]
        public string TestCode { get; set; }
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
}