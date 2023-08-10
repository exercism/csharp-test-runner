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
        public string Name { get; set; }
        public TestStatus Status { get; set; }
        public int? TaskId { get; set; }
        public string Message { get; set; }
        public string Output { get; set; }
        public string TestCode { get; set; }
    }

    internal class TestRun
    {
        public int Version { get; set; } = 3;
        public TestStatus Status { get; set; }
        public string Message { get; set; }
        public TestResult[] Tests { get; set; }
    }
}