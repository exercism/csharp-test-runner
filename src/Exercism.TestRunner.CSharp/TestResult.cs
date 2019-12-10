namespace Exercism.TestRunner.CSharp
{
    internal class TestResult
    {
        public string Name { get; }
        public string Message { get; }
        public TestStatus Status { get; }

        public TestResult(string name, string message, TestStatus status) =>
            (Name, Message, Status) = (name, message, status);
    }
}