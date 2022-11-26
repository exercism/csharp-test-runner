using System.Diagnostics;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    internal static class TestSolutionRunner
    {
        public static TestRun Run(string directory, string slug = "Fake")
        {
            var testSolution = new TestSolution(slug, directory);

            RunTestRunner(testSolution);
            return CreateTestRun(testSolution);
        }

        private static void RunTestRunner(TestSolution testSolution) =>
            Program.Main(new[] { testSolution.Slug, testSolution.DirectoryFullPath, testSolution.DirectoryFullPath });

        private static TestRun CreateTestRun(TestSolution solution)
        {
            var actualTestRunResult = TestRunResultReader.ReadActual(solution);
            var expectedTestRunResult = TestRunResultReader.ReadExpected(solution);

            return new TestRun(expectedTestRunResult, actualTestRunResult);
        }
    }
}