using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CommandLine;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
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

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => CreateTestResults(options).GetAwaiter().GetResult());
        }

        private static async Task CreateTestResults(Options options)
        {
            Log.Information("Running test runner for {Exercise} solution in directory {Directory}", options.Slug, options.InputDirectory);

            var projectFilePath = @"/Users/erik/Code/exercism/csharp-test-runner/test/Exercism.TestRunner.CSharp.IntegrationTests/Solutions/MultipleTestsWithTestOutput/Fake.csproj";
            var projectDirectory = Path.GetDirectoryName(projectFilePath);

            using var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(projectFilePath);

            var testsDocument = project.Documents.Single(document => document.Name.EndsWith("Test.cs"));
            var testsRoot = await testsDocument.GetSyntaxRootAsync();
            testsRoot = new UnskipTests().Visit(testsRoot);
            testsRoot = new CaptureConsoleOutput().Visit(testsRoot);
            testsDocument = testsDocument.WithSyntaxRoot(testsRoot);

            var tryApplyChanges = workspace.TryApplyChanges(testsDocument.Project.Solution);

            var processStartInfo = new ProcessStartInfo("dotnet", "test --verbosity=quiet --logger \"trx;LogFileName=tests.trx\" /flp:v=q");
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.WorkingDirectory = projectDirectory;
            Process.Start(processStartInfo)?.WaitForExit();

            var testRun = CreateTestRun(projectDirectory);

            


            TestRunWriter.WriteToFile(options, testRun);

            Log.Information("Ran test runner for {Exercise} solution in directory {Directory}", options.Slug, options.OutputDirectory);
        }

        private static TestRun CreateTestRun(string projectDirectory)
        {
            var logLines = File.ReadLines(Path.Combine(projectDirectory, "msbuild.log"));
            var buildFailed = logLines.Any();

            if (buildFailed)
            {
                return new TestRun
                {
                    Message = string.Join("\n", logLines),
                    Status = TestStatus.Error
                };
            }

            var testResults = TestRunLog.Parse(projectDirectory);

            //
            // var xDoc = XDocument.Load(
            //     "/Users/erik/Code/exercism/csharp-test-runner/test/Exercism.TestRunner.CSharp.IntegrationTests/Solutions/MultipleTestsWithTestOutput/TestResults/tests.trx");
            //
            // var ns = xDoc.Root.Name.Namespace;
            // var unitTestResults = xDoc.Descendants(ns + "UnitTestResult");
            //
            // var testResults = unitTestResults.Select(result => new TestResult
            // {
            //     Name = result.Attribute(ns + "testName").Value,
            //     Status = result.Attribute(ns + "outcome").Value == "Failed" ? TestStatus.Fail : TestStatus.Pass,
            // }).ToArray();
            //
            var status = testResults.Any(x => x.Status == TestStatus.Error) ? TestStatus.Error : TestStatus.Pass;

            return new TestRun
            {
                Status = status,
                Tests = testResults
            };
        }
    }

    class UnskipTests : CSharpSyntaxRewriter
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

    class CaptureConsoleOutput : CSharpSyntaxRewriter
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