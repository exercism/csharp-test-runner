using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static async Task<TestRun> Run(Compilation compilation) =>
            await Run(compilation.UnskipTests().ToAssembly().ToAssemblyInfo());

        private static async Task<TestRun> Run(IAssemblyInfo assemblyInfo)
        {
            var unorderedTestResults = new List<TestResult>();

            ExecutionMessageSink.Execution.TestFailedEvent += args =>
                unorderedTestResults.Add(TestResult.FromFailed(args.Message));

            ExecutionMessageSink.Execution.TestPassedEvent += args =>
                unorderedTestResults.Add(TestResult.FromPassed(args.Message));

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
            var captureTracesAsTestOutputRewriter = new CaptureTracesAsTestOutputRewriter();

            foreach (var syntaxTree in compilation.SyntaxTrees)
                compilation = compilation.ReplaceSyntaxTree(
                    syntaxTree,
                    syntaxTree.WithRootAndOptions(
                        captureTracesAsTestOutputRewriter.Visit(enableTestsRewriter.Visit(syntaxTree.GetRoot())), syntaxTree.Options));

            return compilation;
        }

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
                classDeclaration.Identifier.Text.EndsWith("Test") &&
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
    }
}