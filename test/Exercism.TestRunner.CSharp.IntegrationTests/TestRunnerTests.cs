using System.IO;
using Xunit;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    public class TestRunnerTests
    {
        [Fact]
        public void SingleCompileError() =>
            AssertSolutionHasExpectedResults("SingleCompileError");

        [Fact]
        public void MultipleCompileErrors() =>
            AssertSolutionHasExpectedResults("MultipleCompileErrors");

        [Fact]
        public void MultipleTestsThatPass() =>
            AssertSolutionHasExpectedResults("MultipleTestsWithAllPasses");

        [Fact]
        public void MultipleTestsAndSingleFail() =>
            AssertSolutionHasExpectedResults("MultipleTestsWithSingleFail");

        [Fact]
        public void MultipleTestsAndMultipleFails() =>
            AssertSolutionHasExpectedResults("MultipleTestsWithMultipleFails");

        [Fact]
        public void MultipleTestsAndTestOutput() =>
            AssertSolutionHasExpectedResults("MultipleTestsWithTestOutput");

        [Fact]
        public void MultipleTestsAndTestOutputExceedingLimit() =>
            AssertSolutionHasExpectedResults("MultipleTestsWithTestOutputExceedingLimit");

        [Fact]
        public void SingleTestThatPasses() =>
            AssertSolutionHasExpectedResults("SingleTestThatPasses");

        [Fact]
        public void SingleTestThatFails() =>
            AssertSolutionHasExpectedResults("SingleTestThatFails");

        [Fact]
        public void NotImplemented() =>
            AssertSolutionHasExpectedResults("NotImplemented");

        [Fact]
        public void NetCoreApp2_1() =>
            AssertSolutionHasExpectedResults("NetCoreApp2.1");

        [Fact]
        public void NetCoreApp2_2() =>
            AssertSolutionHasExpectedResults("NetCoreApp2.2");

        [Fact]
        public void NetCoreApp3_0() =>
            AssertSolutionHasExpectedResults("NetCoreApp3.0");

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