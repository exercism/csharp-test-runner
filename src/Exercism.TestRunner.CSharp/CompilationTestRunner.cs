using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Exercism.TestRunner.CSharp
{
    internal static class CompilationTestRunner
    {
        private static readonly ISourceInformationProvider SourceInformationProvider = new NullSourceInformationProvider();
        private static readonly TestMessageSink DiagnosticMessageSink = new TestMessageSink();
        private static readonly TestMessageSink ExecutionMessageSink = new TestMessageSink();

        public static async Task<TestRun> Run(Compilation compilation) =>
            await Run(compilation.Rewrite().ToAssembly().ToAssemblyInfo());

        private static async Task<TestRun> Run(IAssemblyInfo assemblyInfo)
        {
            var testResults = new List<TestResult>();

            ExecutionMessageSink.Execution.TestFailedEvent += args =>
                testResults.Add(TestResult.FromFailed(args.Message));

            ExecutionMessageSink.Execution.TestPassedEvent += args =>
                testResults.Add(TestResult.FromPassed(args.Message));

            var testCases = TestCases(assemblyInfo);

            using var assemblyRunner = CreateTestAssemblyRunner(testCases, assemblyInfo.ToTestAssembly());
            await assemblyRunner.RunAsync();

            return TestRun.FromTests(testResults.ToArray());
        }

        private static TestAssembly ToTestAssembly(this IAssemblyInfo assemblyInfo) =>
            new TestAssembly(assemblyInfo);

        private static IReflectionAssemblyInfo ToAssemblyInfo(this Assembly assembly) =>
            Reflector.Wrap(assembly);

        private static SequentialTestAssemblyRunner CreateTestAssemblyRunner(IEnumerable<IXunitTestCase> testCases, TestAssembly testAssembly) =>
            new SequentialTestAssemblyRunner(
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

        private class SequentialTestAssemblyRunner : XunitTestAssemblyRunner
        {
            private readonly ITestCaseOrderer _testCaseOrderer = new SequentialOrderer();

            public SequentialTestAssemblyRunner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions) : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
            {
            }

            protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases,
                CancellationTokenSource cancellationTokenSource) =>
                new XunitTestCollectionRunner(testCollection, testCases, DiagnosticMessageSink, messageBus, _testCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource).RunAsync();

            private class SequentialOrderer : ITestCaseOrderer
            {
                public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase =>
                    testCases.ToList();
            }
        }
    }
}