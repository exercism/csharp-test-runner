using System.Diagnostics;
using System.Threading.Tasks;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    internal static class TestSolutionRunner
    {
        public static async Task<TestRun> Run(string directory)
        {
            var testSolution = new TestSolution("Fake", directory);

            await RunTestRunner(testSolution);
            return CreateTestRun(testSolution);
        }

        private static async Task RunTestRunner(TestSolution testSolution)
        {
            if (Options.UseDocker)
                RunTestRunnerUsingDocker(testSolution);
            else
                await RunTestRunnerWithoutDocker(testSolution);
        }

        private static void RunTestRunnerUsingDocker(TestSolution testSolution) =>
            Process.Start("docker", $"run -v {testSolution.DirectoryFullPath}:/solution -v {testSolution.DirectoryFullPath}:/results exercism/csharp-test-runner {testSolution.Slug} /solution /results")!.WaitForExit();

        private static async Task RunTestRunnerWithoutDocker(TestSolution testSolution) =>
            await Program.Main(new[] { testSolution.Slug, testSolution.DirectoryFullPath, testSolution.DirectoryFullPath });

        private static TestRun CreateTestRun(TestSolution solution)
        {
            var actualTestRunResult = TestRunResultReader.ReadActual(solution);
            var expectedTestRunResult = TestRunResultReader.ReadExpected(solution);

            return new TestRun(expectedTestRunResult, actualTestRunResult);
        }
    }
}