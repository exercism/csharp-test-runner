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

        [Fact]
        public void TestsInDifferentFormats()
        {
            var testRun = TestSolutionRunner.Run("TestsInDifferentFormats");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void NoTasks()
        {
            var testRun = TestSolutionRunner.Run("NoTasks");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void AllTestsWithTask()
        {
            var testRun = TestSolutionRunner.Run("AllTestsWithTask");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void SomeTestsWithTask()
        {
            var testRun = TestSolutionRunner.Run("SomeTestsWithTask");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void WithExample()
        {
            var testRun = TestSolutionRunner.Run("WithExample");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void WithConstructor()
        {
            var testRun = TestSolutionRunner.Run("WithConstructor");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void WithDisposable()
        {
            var testRun = TestSolutionRunner.Run("WithDisposable");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void UseCultureAttribute()
        {
            var testRun = TestSolutionRunner.Run("UseCultureAttribute");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void DownloadedSolution()
        {
            var testRun = TestSolutionRunner.Run("DownloadedSolution");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void WithNonTestClasses()
        {
            var testRun = TestSolutionRunner.Run("WithNonTestClasses");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public void WithPrivateDisposableClasses()
        {
            var testRun = TestSolutionRunner.Run("WithPrivateDisposableClasses");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
    }
}