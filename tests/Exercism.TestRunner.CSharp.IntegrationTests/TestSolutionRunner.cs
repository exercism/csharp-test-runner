using System.Diagnostics;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    internal static class TestSolutionRunner
    {
        public static TestRun Run(TestSolution testSolution)
        {
            RunSolutionTestScript(testSolution);
            return CreateTestRun(testSolution);
        }

        private static void RunSolutionTestScript(TestSolution testSolution) =>
            Process.Start("pwsh", $"{DirectoryHelper.FindFileInTree("run.ps1")} {testSolution.Slug} {testSolution.Directory} {testSolution.Directory}")?.WaitForExit();

        private static TestRun CreateTestRun(TestSolution solution)
        {
            var actualTestRunResult = TestRunResultReader.ReadActual(solution);
            var expectedTestRunResult = TestRunResultReader.ReadExpected(solution);

            return new TestRun(expectedTestRunResult, actualTestRunResult);
        }
    }
}