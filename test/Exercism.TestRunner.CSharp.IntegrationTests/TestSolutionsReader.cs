using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    internal static class TestSolutionsReader
    {
        public static IEnumerable<TestSolution> ReadAll() =>
            GetTestSolutionDirectories().Select(CreateTestSolution);

        private static IEnumerable<string> GetTestSolutionDirectories() =>
            Directory.GetDirectories("Solutions");
        private static TestSolution CreateTestSolution(string solutionDirectory) =>
            new TestSolution("Fake", Path.GetFullPath(solutionDirectory));
    }
}