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

        private static void RunTestRunner(TestSolution testSolution)
        {
            if (Options.UseDocker)
                RunTestRunnerUsingDocker(testSolution);
            else
                RunTestRunnerWithoutDocker(testSolution);
        }

        private static void RunTestRunnerUsingDocker(TestSolution testSolution) =>
            Process.Start("docker",
                new[]
                {
                    "run",
                    "--network", "none",
                    "--read-only",
                    "--mount", $"type=bind,src={testSolution.DirectoryFullPath},dst=/solution",
                    "--mount", $"type=bind,src={testSolution.DirectoryFullPath},dst=/output",
                    "--mount", "type=tmpfs,dst=/tmp",
                    "exercism/csharp-test-runner",
                    testSolution.Slug,
                    "/solution",
                    "/output"
                })!.WaitForExit();

        private static void RunTestRunnerWithoutDocker(TestSolution testSolution) =>
            Program.Main(new[] { testSolution.Slug, testSolution.DirectoryFullPath, testSolution.DirectoryFullPath });

        private static TestRun CreateTestRun(TestSolution solution)
        {
            var actualTestRunResult = TestRunResultReader.ReadActual(solution);
            var expectedTestRunResult = TestRunResultReader.ReadExpected(solution);

            return new TestRun(expectedTestRunResult, actualTestRunResult);
        }
    }
}