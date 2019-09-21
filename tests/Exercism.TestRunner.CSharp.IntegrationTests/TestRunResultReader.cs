using System.IO;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    internal static class TestRunResultReader
    {
        public static string ReadActual(TestSolution solution) =>
            ReadFile(solution, "results.json");

        public static string ReadExpected(TestSolution solution) =>
            ReadFile(solution, "expected_results.json");

        private static string ReadFile(TestSolution solution, string fileName) =>
            File.ReadAllText(Path.Combine(solution.Directory, fileName));
    }
}