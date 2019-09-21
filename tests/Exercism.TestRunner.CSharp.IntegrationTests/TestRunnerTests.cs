using System.IO;
using Xunit;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    public class TestRunnerTests
    {
        [Fact]
        public void SolutionWithCompileError() =>
            AssertSolutionHasExpectedResults("CompileError");

        [Fact]
        public void SolutionWithMultipleTestsThatPass() =>
            AssertSolutionHasExpectedResults("MultipleTestsWithAllPasses");

        [Fact]
        public void SolutionWithMultipleTestsAndSingleFail() =>
            AssertSolutionHasExpectedResults("MultipleTestsWithSingleFail");

        [Fact]
        public void SolutionWithMultipleTestsAndMultipleFails() =>
            AssertSolutionHasExpectedResults("MultipleTestsWithMultipleFails");

        [Fact]
        public void SolutionWithSingleTestThatPasses() =>
            AssertSolutionHasExpectedResults("SingleTestThatPasses");

        [Fact]
        public void SolutionWithSingleTestThatFails() =>
            AssertSolutionHasExpectedResults("SingleTestThatFails");

        private static void AssertSolutionHasExpectedResults(string directory)
        {
            var testSolution = CreateTestSolutionFromDirectory(directory);
            var testRun = TestSolutionRunner.Run(testSolution);
            Assert.Equal(testRun.Expected.NormalizeJson(), testRun.Actual.NormalizeJson());
        }

        private static TestSolution CreateTestSolutionFromDirectory(string directory) =>
            new TestSolution("Fake", Path.GetFullPath(Path.Combine(new[] { "Solutions", directory })));
    }
}