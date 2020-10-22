using Xunit;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    public class TestRunnerTests
    {
        [Fact]
        public void MultipleCompileErrors()
        {
            var testRun = TestSolutionRunner.Run("MultipleCompileErrors");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void MultipleTestClassesWithAllPasses()
        {
            var testRun = TestSolutionRunner.Run("MultipleTestClassesWithAllPasses");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void MultipleTestsWithAllPasses()
        {
            var testRun = TestSolutionRunner.Run("MultipleTestsWithAllPasses");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public void MultipleTestsWithMultipleFails()
        {
            var testRun = TestSolutionRunner.Run("MultipleTestsWithMultipleFails");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public void MultipleTestsWithSingleFail()
        {
            var testRun = TestSolutionRunner.Run("MultipleTestsWithSingleFail");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public void MultipleTestsWithTestOutput()
        {
            var testRun = TestSolutionRunner.Run("MultipleTestsWithTestOutput");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
                
        [Fact]
        public void MultipleTestsWithTestOutputExceedingLimit()
        {
            var testRun = TestSolutionRunner.Run("MultipleTestsWithTestOutputExceedingLimit");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public void NetCoreApp3_0()
        {
            var testRun = TestSolutionRunner.Run("NetCoreApp3.0");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public void NetCoreApp3_1()
        {
            var testRun = TestSolutionRunner.Run("NetCoreApp3.1");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public void NoTests()
        {
            var testRun = TestSolutionRunner.Run("NoTests");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public void NotImplemented()
        {
            var testRun = TestSolutionRunner.Run("NotImplemented");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public void SingleCompileError()
        {
            var testRun = TestSolutionRunner.Run("SingleCompileError");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public void SingleTestThatFails()
        {
            var testRun = TestSolutionRunner.Run("SingleTestThatFails");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void SingleTestThatPasses()
        {
            var testRun = TestSolutionRunner.Run("SingleTestThatPasses");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void SingleTestThatPassesWithDifferentSlug()
        {
            var testRun = TestSolutionRunner.Run("SingleTestThatPassesWithDifferentSlug", "Foo");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
    }
}