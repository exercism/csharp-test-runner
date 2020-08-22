using System.Diagnostics;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    internal static class TestSolutionRunner
    {
        public static TestRun Run(TestSolution testSolution)
        {
            RunTestRunner(testSolution);
            return CreateTestRun(testSolution);
        }

        private static void RunTestRunner(TestSolution testSolution)
        {
            if (Options.UseDocker)
                RunTestRunnerUsingDocker(testSolution);
            else
                RunTestRunnerWithoutDocker(testSolution);
        }

        private static void RunTestRunnerUsingDocker(TestSolution testSolution) =>
            Process.Start("docker", $"run -v {testSolution.Directory}:/solution -v {testSolution.Directory}:/results exercism/csharp-test-runner {testSolution.Slug} /solution /results").WaitForExit();

        private static void RunTestRunnerWithoutDocker(TestSolution testSolution) =>
            Program.Main(new[] { testSolution.Slug, testSolution.Directory, testSolution.Directory });

        private static TestRun CreateTestRun(TestSolution solution)
        {
            var actualTestRunResult = TestRunResultReader.ReadActual(solution);
            var expectedTestRunResult = TestRunResultReader.ReadExpected(solution);

            return new TestRun(expectedTestRunResult, actualTestRunResult);
        }
    }
}