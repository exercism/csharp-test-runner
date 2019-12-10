using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Exercism.TestRunner.CSharp
{
    internal static class InMemoryXunitTestRunner
    {
        private static readonly ISourceInformationProvider SourceInformationProvider = new NullSourceInformationProvider();
        private static readonly TestMessageSink DiagnosticMessageSink = new TestMessageSink();
        private static readonly TestMessageSink ExecutionMessageSink = new TestMessageSink();

        public static async Task<TestRun> RunAllTests(Assembly assembly)
        {
            var unorderedTestResults = new List<TestResult>();

            ExecutionMessageSink.Execution.TestFailedEvent += args =>
                unorderedTestResults.Add(new TestResult(args.Message.Test.DisplayName, string.Join("\n", args.Message.Messages).Replace("\r\n", "\n"), TestStatus.Fail));
            
            ExecutionMessageSink.Execution.TestPassedEvent += args =>
                unorderedTestResults.Add(new TestResult(args.Message.Test.DisplayName, null, TestStatus.Pass));

            var assemblyInfo = GetAssemblyInfo(assembly);
            var testCases = TestCases(assemblyInfo);
            
            using var assemblyRunner = CreateTestAssemblyRunner(testCases, CreateTestAssembly(assemblyInfo));
            await assemblyRunner.RunAsync();
            
            var status = unorderedTestResults.Any(testResult => testResult.Status == TestStatus.Fail)
                ?  TestStatus.Fail
                : unorderedTestResults.All(testResultEvent => testResultEvent.Status == TestStatus.Pass)
                    ? TestStatus.Pass
                    : TestStatus.Error;

            var orderedTestResults = testCases
                .Select(testCase => unorderedTestResults.First(testResult => testResult.Name == testCase.DisplayName))
                .ToArray();

            return new TestRun(null, status, orderedTestResults);
        }

        private static TestAssembly CreateTestAssembly(IReflectionAssemblyInfo assemblyInfo) =>
            new TestAssembly(assemblyInfo);

        private static IReflectionAssemblyInfo GetAssemblyInfo(Assembly assembly) =>
            Reflector.Wrap(assembly);

        private static XunitTestAssemblyRunner CreateTestAssemblyRunner(IEnumerable<IXunitTestCase> testCases, TestAssembly testAssembly) =>
            new XunitTestAssemblyRunner(
                testAssembly,
                testCases,
                DiagnosticMessageSink,
                ExecutionMessageSink,
                TestFrameworkOptions.ForExecution());

        private static IXunitTestCase[] TestCases(IAssemblyInfo assemblyInfo)
        {
            using var discoverySink = new TestDiscoverySink();
            using var discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink);

            discoverer.Find(false, discoverySink, TestFrameworkOptions.ForDiscovery());
            discoverySink.Finished.WaitOne();

            return discoverySink.TestCases.Cast<IXunitTestCase>().ToArray();
        }
    }
}