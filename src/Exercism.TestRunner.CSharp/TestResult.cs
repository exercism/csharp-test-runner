namespace Exercism.TestRunner.CSharp
{
    public class TestResult
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public TestStatus Status { get; set; }
    }
}