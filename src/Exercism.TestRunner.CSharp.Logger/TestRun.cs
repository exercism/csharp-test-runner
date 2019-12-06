namespace Microsoft.VisualStudio.TestPlatform.Extension.Exercism.TestLogger
{
    public class TestRun
    {
        public string Message { get; }
        public TestStatus Status { get; }
        public TestResult[] Tests { get; }

        public TestRun(string message, TestStatus status, TestResult[] tests)
        {
            Message = message;
            Status = status;
            Tests = tests;
//            (Name, Message, Status) = (name, message, status);
        }
    }
}