using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Humanizer;
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

            var exercise = options.Slug.Dehumanize().Pascalize();
            var projectFilePath = Path.Combine(options.InputDirectory, $"{exercise}.csproj");
            var projectDirectory = Path.GetDirectoryName(projectFilePath);

            using var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(projectFilePath);

            var testsDocument = project.Documents.Single(document => document.Name.EndsWith("Test.cs"));
            var testsRoot = await testsDocument.GetSyntaxRootAsync();
            testsRoot = new TestProjectRewriter.UnskipTests().Visit(testsRoot);
            testsRoot = new TestProjectRewriter.CaptureConsoleOutput().Visit(testsRoot);
            var updatedTestsDocument = testsDocument.WithSyntaxRoot(testsRoot);

            var tryApplyChanges = project.Solution.Workspace.TryApplyChanges(updatedTestsDocument.Project.Solution);

            var processStartInfo = new ProcessStartInfo("dotnet", "test --verbosity=quiet --logger \"trx;LogFileName=tests.trx\" /flp:v=q");
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.WorkingDirectory = projectDirectory;
            Process.Start(processStartInfo)?.WaitForExit();

            var testRun = TestRunParser.FromFile(projectDirectory);
            TestRunWriter.WriteToFile(testRun, options);

            Log.Information("Ran test runner for {Exercise} solution in directory {Directory}", options.Slug, options.OutputDirectory);
        }
    }

    internal class TestProjectRewriter
    {
        

        public class UnskipTests : CSharpSyntaxRewriter
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

        public class CaptureConsoleOutput : CSharpSyntaxRewriter
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