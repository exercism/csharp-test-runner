using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Exercism.TestRunner.CSharp
{
    internal static class CompilationTestRunner
    {
        private static readonly ISourceInformationProvider SourceInformationProvider = new NullSourceInformationProvider();
        private static readonly TestMessageSink DiagnosticMessageSink = new TestMessageSink();
        private static readonly TestMessageSink ExecutionMessageSink = new TestMessageSink();

        public static async Task<TestRun> Run(Compilation compilation)
        {
            var optimizedCompilation = compilation
                .UnskipTests()
                .CaptureTracesAsTestOutput();

            return await Run(optimizedCompilation.ToAssembly().ToAssemblyInfo());
        }

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

        private static SequentialXunitTestAssemblyRunner CreateTestAssemblyRunner(IEnumerable<IXunitTestCase> testCases, TestAssembly testAssembly) =>
            new SequentialXunitTestAssemblyRunner(
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

        private static Compilation UnskipTests(this Compilation compilation) =>
            compilation.Rewrite(new UnskipTestsRewriter());

        private static Compilation CaptureTracesAsTestOutput(this Compilation compilation) =>
            compilation.Rewrite(new CaptureTracesAsTestOutputRewriter());

        private class UnskipTestsRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitAttributeArgument(AttributeArgumentSyntax node)
            {
                if (IsSkipAttributeArgument(node))
                    return null;

                return base.VisitAttributeArgument(node);
            }

            private static bool IsSkipAttributeArgument(AttributeArgumentSyntax node) =>
                node.NameEquals?.Name.ToString() == "Skip";
        }

        private class CaptureTracesAsTestOutputRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
            {
                if (node.DescendantNodes().Any(IsTestClass))
                    return base.VisitCompilationUnit(
                        node.WithUsings(
                            node.Usings.Add(
                                UsingDirective(QualifiedName(
                                IdentifierName("Xunit").WithLeadingTrivia(Space),
                                IdentifierName("Abstractions"))))));

                return base.VisitCompilationUnit(node);
            }

            private static bool IsTestClass(SyntaxNode descendant) =>
                descendant is ClassDeclarationSyntax classDeclaration &&
                classDeclaration.Identifier.Text.EndsWith("Test");

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (IsTestClass(node))
                    return base.VisitClassDeclaration(
                        node.WithBaseList(
                                BaseList(
                                    SingletonSeparatedList<BaseTypeSyntax>(
                                        SimpleBaseType(
                                            IdentifierName("TracingTestBase")))))
                            .WithMembers(node.Members.Insert(0, ConstructorDeclaration(
                                    Identifier("FakeTest"))
                                .WithModifiers(
                                    TokenList(
                                        Token(SyntaxKind.PublicKeyword).WithTrailingTrivia(Space)))
                                .WithParameterList(
                                    ParameterList(
                                        SingletonSeparatedList<ParameterSyntax>(
                                            Parameter(
                                                    Identifier("output"))
                                                .WithType(
                                                    IdentifierName("ITestOutputHelper").WithTrailingTrivia(Space)))))
                                .WithInitializer(
                                    ConstructorInitializer(
                                        SyntaxKind.BaseConstructorInitializer,
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    IdentifierName("output"))))))
                                .WithBody(
                                    Block()))));

                return base.VisitClassDeclaration(node);
            }
        }
        
        private class SequentialOrderer : ITestCaseOrderer
        {
            public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase =>
                testCases.ToList();
        }
    
        private class SequentialXunitTestAssemblyRunner : XunitTestAssemblyRunner
        {
            private readonly ITestCaseOrderer _testCaseOrderer = new SequentialOrderer();

            public SequentialXunitTestAssemblyRunner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions) : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
            {
            }   

            protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases,
                CancellationTokenSource cancellationTokenSource) =>
                new XunitTestCollectionRunner(testCollection, testCases, DiagnosticMessageSink, messageBus, _testCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource).RunAsync();

        }
    }
}