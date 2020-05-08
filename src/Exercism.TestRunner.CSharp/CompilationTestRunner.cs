using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection; 
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

            var orderedTestNames = testCases.Select(testCase => testCase.DisplayName).ToArray();
            var orderedTestResults = testResults.OrderBy(testResult => Array.IndexOf(orderedTestNames, testResult.Test)).ToArray();

            return TestRun.FromTests(orderedTestResults);
        }

        private static TestAssembly ToTestAssembly(this IAssemblyInfo assemblyInfo) =>
            new TestAssembly(assemblyInfo);

        private static IReflectionAssemblyInfo ToAssemblyInfo(this Assembly assembly) =>
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