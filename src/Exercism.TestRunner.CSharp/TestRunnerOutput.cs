namespace Exercism.TestRunner.CSharp
{
    internal class TestRunnerOutput
    {
        private readonly string _output;

        public TestRunnerOutput(string output) => _output = output;

        public string Normalized => _output;
    }
}