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
            await Run(compilation.UnskipTests().ToAssembly().ToAssemblyInfo());

        private static async Task<TestRun> Run(IAssemblyInfo assemblyInfo)
        {
            var unorderedTestResults = new List<TestResult>();

            ExecutionMessageSink.Execution.TestFailedEvent += args =>
                unorderedTestResults.Add(new TestResult(args.Message.Test.DisplayName, TestRunMessage.FromFailedTest(args.Message), TestStatus.Fail));
            
            ExecutionMessageSink.Execution.TestPassedEvent += args =>
                unorderedTestResults.Add(new TestResult(args.Message.Test.DisplayName, null, TestStatus.Pass));

            var testCases = TestCases(assemblyInfo);
            
            using var assemblyRunner = CreateTestAssemblyRunner(testCases, assemblyInfo.ToTestAssembly());
            await assemblyRunner.RunAsync();

            var orderedTestResults = testCases
                .Select(testCase => unorderedTestResults.First(testResult => testResult.Name == testCase.DisplayName))
                .ToArray();

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

        private static Compilation UnskipTests(this Compilation compilation)
        {
            var enableTestsRewriter = new UnskipTestsRewriter();
            
            foreach (var syntaxTree in compilation.SyntaxTrees)
                compilation = compilation.ReplaceSyntaxTree(
                    syntaxTree,
                    syntaxTree.WithRootAndOptions(enableTestsRewriter.Visit(syntaxTree.GetRoot()), syntaxTree.Options));

            return compilation;
        }
    }
}