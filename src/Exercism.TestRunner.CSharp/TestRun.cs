namespace Exercism.TestRunner.CSharp
{
    internal class TestRun
    {
        public string Message { get; }
        public TestStatus Status { get; }
        public TestResult[] Tests { get; }

        public TestRun(string message, TestStatus status, TestResult[] tests) =>
            (Message, Status, Tests) = (message, status, tests);
    }
}