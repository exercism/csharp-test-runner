using System.IO;
using System.Linq;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunParser
    {
        public static TestRun ReadFromFile(string projectDirectory)
        {
            var logLines = File.ReadLines(Path.Combine(projectDirectory, "msbuild.log"));
            var buildFailed = logLines.Any();

            if (buildFailed)
            {
                return new TestRun
                {
                    Message = string.Join("\n", logLines),
                    Status = TestStatus.Error
                };
            }

            var testResults = TestResultParser.FromFile(Path.Combine(projectDirectory, "TestResults", "tests.trx"));
            var status = testResults.Any(x => x.Status == TestStatus.Error) ? TestStatus.Error : TestStatus.Pass;

            return new TestRun
            {
                Status = status,
                Tests = testResults
            };
        }
    }
}