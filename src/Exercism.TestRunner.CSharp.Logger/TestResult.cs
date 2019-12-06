namespace Microsoft.VisualStudio.TestPlatform.Extension.Exercism.TestLogger
{
    public class TestResult
    {
        public string Name { get; }
        public string Message { get; }
        public TestStatus Status { get; }

        public TestResult(string name, string message, TestStatus status)
        {
            Name = name;
            Message = message;
            Status = status;
//            (Name, Message, Status) = (name, message, status);
        }
    }
}