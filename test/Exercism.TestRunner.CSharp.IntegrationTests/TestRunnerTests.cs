using Xunit;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    public class TestRunnerTests
    {
        [Theory]
        [TestSolutionsData]
        public void SolutionIsTestedCorrectly(TestSolution solution)
        {
            var testRun = TestSolutionRunner.Run(solution);
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
    }
}