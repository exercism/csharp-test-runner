using Xunit.Abstractions;

namespace Exercism.TestRunner.CSharp
{
    internal class TestResult
    {
        public string Name { get; }
        public string Message { get; }
        public string Output { get; }
        public TestStatus Status { get; }

        private TestResult(string name, TestStatus status, string message, string output) =>
            (Name, Message, Status, Output) = (name, message, status, output);

        public static TestResult FromPassed(ITestPassed test) =>
            new TestResult(test.TestCase.DisplayName, TestStatus.Pass, null, test.Output);

        public static TestResult FromFailed(ITestFailed test) =>
            new TestResult(test.TestCase.DisplayName, TestStatus.Fail, TestRunMessage.FromMessages(test.Messages), test.Output);
    }
}