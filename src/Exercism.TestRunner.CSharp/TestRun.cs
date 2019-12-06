namespace Exercism.TestRunner.CSharp
{
    public class TestRun
    {
        public string Message { get; set; }
        public TestStatus Status { get; set; }
        public TestResult[] Tests { get; set; }
    }
}