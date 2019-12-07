namespace Exercism.TestRunner.CSharp
{
    internal class TestRunnerOutput
    {
        private readonly string _output;

        public TestRunnerOutput(string output, bool success) =>
            (_output, Success) = (output, success);

        public bool Success { get; }

        public string Normalized => _output;
    }
}