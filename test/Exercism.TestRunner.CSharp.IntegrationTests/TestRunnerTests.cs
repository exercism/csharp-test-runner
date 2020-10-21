using System.Threading.Tasks;
using Xunit;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    public class TestRunnerTests
    {
        [Fact]
        public async Task MultipleCompileErrors()
        {
            var testRun = await TestSolutionRunner.Run("MultipleCompileErrors");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public async Task MultipleTestClassesWithAllPasses()
        {
            var testRun = await TestSolutionRunner.Run("MultipleTestClassesWithAllPasses");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public async Task MultipleTestsWithAllPasses()
        {
            var testRun = await TestSolutionRunner.Run("MultipleTestsWithAllPasses");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public async Task MultipleTestsWithMultipleFails()
        {
            var testRun = await TestSolutionRunner.Run("MultipleTestsWithMultipleFails");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public async Task MultipleTestsWithSingleFail()
        {
            var testRun = await TestSolutionRunner.Run("MultipleTestsWithSingleFail");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public async Task MultipleTestsWithTestOutput()
        {
            var testRun = await TestSolutionRunner.Run("MultipleTestsWithTestOutput");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
                
        [Fact]
        public async Task MultipleTestsWithTestOutputExceedingLimit()
        {
            var testRun = await TestSolutionRunner.Run("MultipleTestsWithTestOutputExceedingLimit");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public async Task NetCoreApp2_1()
        {
            var testRun = await TestSolutionRunner.Run("NetCoreApp2.1");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public async Task NetCoreApp2_2()
        {
            var testRun = await TestSolutionRunner.Run("NetCoreApp2.2");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public async Task NetCoreApp3_0()
        {
            var testRun = await TestSolutionRunner.Run("NetCoreApp3.0");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public async Task NoTests()
        {
            var testRun = await TestSolutionRunner.Run("NoTests");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public async Task NotImplemented()
        {
            var testRun = await TestSolutionRunner.Run("NotImplemented");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public async Task SingleCompileError()
        {
            var testRun = await TestSolutionRunner.Run("SingleCompileError");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
        
        [Fact]
        public async Task SingleTestThatFails()
        {
            var testRun = await TestSolutionRunner.Run("SingleTestThatFails");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }

        [Fact]
        public async Task SingleTestThatPasses()
        {
            var testRun = await TestSolutionRunner.Run("SingleTestThatPasses");
            Assert.Equal(testRun.Expected, testRun.Actual);
        }
    }
}