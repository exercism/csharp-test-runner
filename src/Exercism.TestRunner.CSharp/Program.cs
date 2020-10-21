using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Serilog;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Exercism.TestRunner.CSharp
{
    public static class Program
    {   
        public static void Main(string[] args)
        {   
            if (!MSBuildLocator.IsRegistered)
                MSBuildLocator.RegisterDefaults();
            
            Logging.Configure();

            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsedAsync(CreateTestResults);
        }

        private static async Task CreateTestResults(Options options)
        {
            Log.Information("Running test runner for {Exercise} solution in directory {Directory}", options.Slug, options.InputDirectory);

            var testProject = await TestProjectReader.FromOptions(options);
            await TestRunner.RunTests(testProject);

            var testRun = TestRunParser.ReadFromFile(Path.GetDirectoryName(testProject.Project.FilePath)!);
            
            var resultsJsonFilePath = Path.GetFullPath(Path.Combine(options.OutputDirectory, "results.json"));
            TestRunWriter.WriteToFile(testRun, resultsJsonFilePath);

            Log.Information("Ran test runner for {Exercise} solution in directory {Directory}", options.Slug, options.OutputDirectory);
        }
    }

    internal class TestRunner
    {
        public static async Task RunTests(TestProject testProject)
        {
            try
            {
                await TestProjectRewriter.Rewrite(testProject);
                RunDotnetTest(testProject);
            }
            finally
            {
                TestProjectRewriter.UndoRewrite(testProject);
            }
        }

        private static void RunDotnetTest(TestProject testProject)
        {
            var processStartInfo = new ProcessStartInfo("dotnet",
                "test --verbosity=quiet --logger \"trx;LogFileName=tests.trx\" /flp:v=q");
            processStartInfo.WorkingDirectory = Path.GetDirectoryName(testProject.Project.FilePath)!;
            Process.Start(processStartInfo)?.WaitForExit();
        }
    }

    internal static class TestProjectRewriter
    {
        public static async Task Rewrite(TestProject testProject)
        {
            var testsRoot = await testProject.TestsDocument().GetSyntaxRootAsync();

            var rewrittenTestsRoot = testsRoot.UnskipTests().CaptureConsoleOutput();
            var rewrittenTestsDocument = testProject.TestsDocument().WithSyntaxRoot(rewrittenTestsRoot);

            testProject.Project.Solution.Workspace.TryApplyChanges(rewrittenTestsDocument.Project.Solution);
        }

        public static void UndoRewrite(TestProject testProject) =>
            testProject.Project.Solution.Workspace.TryApplyChanges(testProject.Project.Solution);

        private static SyntaxNode UnskipTests(this SyntaxNode testsRoot) =>
            new UnskipTestsRewriter().Visit(testsRoot);

        private static SyntaxNode CaptureConsoleOutput(this SyntaxNode testsRoot) =>
            new CaptureConsoleOutputRewriter().Visit(testsRoot);

        private class UnskipTestsRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitAttribute(AttributeSyntax node)
            {
                if (node.Name.ToString() == "Fact")
                {
                    return base.VisitAttribute(node.WithArgumentList(null));
                }

                return base.VisitAttribute(node);
            }
        }

        private class CaptureConsoleOutputRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) => 
                base.VisitClassDeclaration(
                    node.WithBaseList(BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(
                                QualifiedName(
                                    IdentifierName("System"),
                                    IdentifierName("IDisposable"))))))
                        .AddMembers(
                            FieldDeclaration(
                                VariableDeclaration(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("Xunit"),
                                                IdentifierName("Abstractions")),
                                            IdentifierName("ITestOutputHelper")))
                                    .WithVariables(
                                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                                            VariableDeclarator(
                                                Identifier("_testOutput"))))),
                            FieldDeclaration(
                                VariableDeclaration(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("IO")),
                                            IdentifierName("StringWriter")))
                                    .WithVariables(
                                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                                            VariableDeclarator(
                                                Identifier("_stringWriter"))))),
                            ConstructorDeclaration(
                            Identifier("FakeTest"))
                        .WithModifiers(
                            TokenList(
                                Token(SyntaxKind.PublicKeyword)))
                        .WithParameterList(
                            ParameterList(
                                SingletonSeparatedList<ParameterSyntax>(
                                    Parameter(
                                        Identifier("testOutput"))
                                    .WithType(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("Xunit"),
                                                IdentifierName("Abstractions")),
                                            IdentifierName("ITestOutputHelper"))))))
                        .WithBody(
                            Block(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("_testOutput"),
                                        IdentifierName("testOutput"))),
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("_stringWriter"),
                                        ObjectCreationExpression(
                                            QualifiedName(
                                                QualifiedName(
                                                    IdentifierName("System"),
                                                    IdentifierName("IO")),
                                                IdentifierName("StringWriter")))
                                        .WithArgumentList(
                                            ArgumentList()))),
                                ExpressionStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("System"),
                                                IdentifierName("Console")),
                                            IdentifierName("SetOut")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    IdentifierName("_stringWriter")))))),
                                ExpressionStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("System"),
                                                IdentifierName("Console")),
                                            IdentifierName("SetError")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    IdentifierName("_stringWriter")))))),
                                ExpressionStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("System"),
                                                        IdentifierName("Diagnostics")),
                                                    IdentifierName("Trace")),
                                                IdentifierName("Listeners")),
                                            IdentifierName("Add")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    ObjectCreationExpression(
                                                        QualifiedName(
                                                            QualifiedName(
                                                                IdentifierName("System"),
                                                                IdentifierName("Diagnostics")),
                                                            IdentifierName("ConsoleTraceListener")))
                                                    .WithArgumentList(
                                                        ArgumentList())))))))),
                            MethodDeclaration(
                            PredefinedType(
                                Token(SyntaxKind.VoidKeyword)),
                            Identifier("Dispose"))
                        .WithModifiers(
                            TokenList(
                                Token(SyntaxKind.PublicKeyword)))
                        .WithBody(
                            Block(
                                LocalDeclarationStatement(
                                    VariableDeclaration(
                                        IdentifierName("var"))
                                    .WithVariables(
                                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                                            VariableDeclarator(
                                                Identifier("output"))
                                            .WithInitializer(
                                                EqualsValueClause(
                                                    InvocationExpression(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("_stringWriter"),
                                                            IdentifierName("ToString")))))))),
                                IfStatement(
                                    BinaryExpression(
                                        SyntaxKind.GreaterThanExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("output"),
                                            IdentifierName("Length")),
                                        LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            Literal(500))),
                                    Block(
                                        SingletonList<StatementSyntax>(
                                            ExpressionStatement(
                                                AssignmentExpression(
                                                    SyntaxKind.SimpleAssignmentExpression,
                                                    IdentifierName("output"),
                                                    BinaryExpression(
                                                        SyntaxKind.AddExpression,
                                                        InvocationExpression(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("output"),
                                                                IdentifierName("Substring")))
                                                        .WithArgumentList(
                                                            ArgumentList(
                                                                SeparatedList<ArgumentSyntax>(
                                                                    new SyntaxNodeOrToken[]{
                                                                        Argument(
                                                                            LiteralExpression(
                                                                                SyntaxKind.NumericLiteralExpression,
                                                                                Literal(0))),
                                                                        Token(SyntaxKind.CommaToken),
                                                                        Argument(
                                                                            LiteralExpression(
                                                                                SyntaxKind.NumericLiteralExpression,
                                                                                Literal(450)))}))),
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(@"

    Output was truncated. Please limit to 500 chars.")))))))),
                                ExpressionStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("_testOutput"),
                                            IdentifierName("WriteLine")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    IdentifierName("output"))))))))).NormalizeWhitespace());
            
            
        }
    }    
}