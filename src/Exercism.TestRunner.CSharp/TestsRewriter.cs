using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestsRewriter
    {
        public static SyntaxTree Rewrite(this SyntaxTree tree) =>
            tree.WithRootAndOptions(tree.GetRoot().Rewrite(), tree.Options);

        private static SyntaxNode Rewrite(this SyntaxNode node) =>
            node.UnskipTests().CaptureConsoleOutput();

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
                    node.WithBaseList(SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                                SyntaxFactory.SimpleBaseType(
                                    SyntaxFactory.QualifiedName(
                                        SyntaxFactory.IdentifierName("System"),
                                        SyntaxFactory.IdentifierName("IDisposable"))))))
                        .AddMembers(
                            SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                        SyntaxFactory.QualifiedName(
                                            SyntaxFactory.QualifiedName(
                                                SyntaxFactory.IdentifierName("Xunit"),
                                                SyntaxFactory.IdentifierName("Abstractions")),
                                            SyntaxFactory.IdentifierName("ITestOutputHelper")))
                                    .WithVariables(
                                        SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                            SyntaxFactory.VariableDeclarator(
                                                SyntaxFactory.Identifier("_testOutput"))))),
                            SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                        SyntaxFactory.QualifiedName(
                                            SyntaxFactory.QualifiedName(
                                                SyntaxFactory.IdentifierName("System"),
                                                SyntaxFactory.IdentifierName("IO")),
                                            SyntaxFactory.IdentifierName("StringWriter")))
                                    .WithVariables(
                                        SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                            SyntaxFactory.VariableDeclarator(
                                                SyntaxFactory.Identifier("_stringWriter"))))),
                            SyntaxFactory.ConstructorDeclaration(
                                    Identifier(node.Identifier.Text))
                                .WithModifiers(
                                    SyntaxFactory.TokenList(
                                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                .WithParameterList(
                                    SyntaxFactory.ParameterList(
                                        SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                                            SyntaxFactory.Parameter(
                                                    SyntaxFactory.Identifier("testOutput"))
                                                .WithType(
                                                    SyntaxFactory.QualifiedName(
                                                        SyntaxFactory.QualifiedName(
                                                            SyntaxFactory.IdentifierName("Xunit"),
                                                            SyntaxFactory.IdentifierName("Abstractions")),
                                                        SyntaxFactory.IdentifierName("ITestOutputHelper"))))))
                                .WithBody(
                                    SyntaxFactory.Block(
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                SyntaxFactory.IdentifierName("_testOutput"),
                                                SyntaxFactory.IdentifierName("testOutput"))),
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                SyntaxFactory.IdentifierName("_stringWriter"),
                                                SyntaxFactory.ObjectCreationExpression(
                                                        SyntaxFactory.QualifiedName(
                                                            SyntaxFactory.QualifiedName(
                                                                SyntaxFactory.IdentifierName("System"),
                                                                SyntaxFactory.IdentifierName("IO")),
                                                            SyntaxFactory.IdentifierName("StringWriter")))
                                                    .WithArgumentList(
                                                        SyntaxFactory.ArgumentList()))),
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.IdentifierName("System"),
                                                            SyntaxFactory.IdentifierName("Console")),
                                                        SyntaxFactory.IdentifierName("SetOut")))
                                                .WithArgumentList(
                                                    SyntaxFactory.ArgumentList(
                                                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName("_stringWriter")))))),
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.IdentifierName("System"),
                                                            SyntaxFactory.IdentifierName("Console")),
                                                        SyntaxFactory.IdentifierName("SetError")))
                                                .WithArgumentList(
                                                    SyntaxFactory.ArgumentList(
                                                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName("_stringWriter")))))),
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName("System"),
                                                                    SyntaxFactory.IdentifierName("Diagnostics")),
                                                                SyntaxFactory.IdentifierName("Trace")),
                                                            SyntaxFactory.IdentifierName("Listeners")),
                                                        SyntaxFactory.IdentifierName("Add")))
                                                .WithArgumentList(
                                                    SyntaxFactory.ArgumentList(
                                                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.ObjectCreationExpression(
                                                                        SyntaxFactory.QualifiedName(
                                                                            SyntaxFactory.QualifiedName(
                                                                                SyntaxFactory.IdentifierName("System"),
                                                                                SyntaxFactory.IdentifierName("Diagnostics")),
                                                                            SyntaxFactory.IdentifierName("ConsoleTraceListener")))
                                                                    .WithArgumentList(
                                                                        SyntaxFactory.ArgumentList())))))))),
                            SyntaxFactory.MethodDeclaration(
                                    SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                                    SyntaxFactory.Identifier("Dispose"))
                                .WithModifiers(
                                    SyntaxFactory.TokenList(
                                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                .WithBody(
                                    SyntaxFactory.Block(
                                        SyntaxFactory.LocalDeclarationStatement(
                                            SyntaxFactory.VariableDeclaration(
                                                    SyntaxFactory.IdentifierName("var"))
                                                .WithVariables(
                                                    SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                                        SyntaxFactory.VariableDeclarator(
                                                                SyntaxFactory.Identifier("output"))
                                                            .WithInitializer(
                                                                SyntaxFactory.EqualsValueClause(
                                                                    SyntaxFactory.InvocationExpression(
                                                                        SyntaxFactory.MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            SyntaxFactory.IdentifierName("_stringWriter"),
                                                                            SyntaxFactory.IdentifierName("ToString")))))))),
                                        SyntaxFactory.IfStatement(
                                            SyntaxFactory.BinaryExpression(
                                                SyntaxKind.GreaterThanExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName("output"),
                                                    SyntaxFactory.IdentifierName("Length")),
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression,
                                                    SyntaxFactory.Literal(500))),
                                            SyntaxFactory.Block(
                                                SyntaxFactory.SingletonList<StatementSyntax>(
                                                    SyntaxFactory.ExpressionStatement(
                                                        SyntaxFactory.AssignmentExpression(
                                                            SyntaxKind.SimpleAssignmentExpression,
                                                            SyntaxFactory.IdentifierName("output"),
                                                            SyntaxFactory.BinaryExpression(
                                                                SyntaxKind.AddExpression,
                                                                SyntaxFactory.InvocationExpression(
                                                                        SyntaxFactory.MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            SyntaxFactory.IdentifierName("output"),
                                                                            SyntaxFactory.IdentifierName("Substring")))
                                                                    .WithArgumentList(
                                                                        SyntaxFactory.ArgumentList(
                                                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                                                new SyntaxNodeOrToken[]{
                                                                                    SyntaxFactory.Argument(
                                                                                        SyntaxFactory.LiteralExpression(
                                                                                            SyntaxKind.NumericLiteralExpression,
                                                                                            SyntaxFactory.Literal(0))),
                                                                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                                    SyntaxFactory.Argument(
                                                                                        SyntaxFactory.LiteralExpression(
                                                                                            SyntaxKind.NumericLiteralExpression,
                                                                                            SyntaxFactory.Literal(450)))}))),
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.StringLiteralExpression,
                                                                    SyntaxFactory.Literal(@"

    Output was truncated. Please limit to 500 chars.")))))))),
                                        SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.IdentifierName("_testOutput"),
                                                        SyntaxFactory.IdentifierName("WriteLine")))
                                                .WithArgumentList(
                                                    SyntaxFactory.ArgumentList(
                                                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName("output"))))))))).NormalizeWhitespace());


        }
    }
}