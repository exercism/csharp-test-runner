using Xunit.Abstractions;

namespace Exercism.TestRunner.CSharp
{
    internal class TestResult
    {
        public string Name { get; }
        public string Message { get; }
        public TestStatus Status { get; }

        private TestResult(string name, TestStatus status, string message) =>
            (Name, Message, Status) = (name, message, status);

        public static TestResult FromPassed(ITestPassed test) =>
            new TestResult(test.TestCase.DisplayName, TestStatus.Pass, null);

        public static TestResult FromFailed(ITestFailed test) =>
            new TestResult(test.TestCase.DisplayName, TestStatus.Fail, TestRunMessage.FromMessages(test.Messages));
    }
}