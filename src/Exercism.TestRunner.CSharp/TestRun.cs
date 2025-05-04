using System.Text.Json.Serialization;

namespace Exercism.TestRunner.CSharp;

internal enum TestStatus
{
    Pass,
    Fail,
    Error
}

internal sealed class TestResult
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("status")]
    public required TestStatus Status { get; set; }
        
    [JsonPropertyName("task_id")]
    public int? TaskId { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("output")]
    public string? Output { get; set; }

    [JsonPropertyName("test_code")]
    public required string TestCode { get; set; }
}

internal sealed class TestRun
{
    [JsonPropertyName("version")]
    public int Version { get; set; } = 3;
    
    [JsonPropertyName("status")]
    public required TestStatus Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("tests")]
    public required TestResult[] Tests { get; set; }
}