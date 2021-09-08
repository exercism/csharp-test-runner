using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestsRewriter
    {
        public static SyntaxTree Rewrite(this SyntaxTree tree) =>
            tree.WithRootAndOptions(tree.GetRoot().Rewrite(), tree.Options);

        private static SyntaxNode Rewrite(this SyntaxNode node) =>
            node.UnskipTests()
                .CaptureConsoleOutput()
                .NormalizeWhitespace();

        private static SyntaxNode UnskipTests(this SyntaxNode testsRoot) =>
            new UnskipTestsRewriter().Visit(testsRoot);

        private static SyntaxNode CaptureConsoleOutput(this SyntaxNode testsRoot) =>
            new CaptureConsoleOutputRewriter().Visit(testsRoot);

        private class UnskipTestsRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitAttribute(AttributeSyntax node)
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
            public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node) =>
                base.VisitConstructorDeclaration(
                    node.AddParameterListParameters(
                        Parameter(Identifier("testOutput"))
                            .WithType(
                                QualifiedName(
                                    QualifiedName(
                                        IdentifierName("Xunit"),
                                        IdentifierName("Abstractions")),
                                    IdentifierName("ITestOutputHelper"))))
                        .AddBodyStatements(
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
                                            SingletonSeparatedList(
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
                                            SingletonSeparatedList(
                                                Argument(
                                                    IdentifierName("_stringWriter"))))))));

            public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (node.Identifier.Text == "Dispose")
                    return base.VisitMethodDeclaration(
                        node.AddBodyStatements(LocalDeclarationStatement(
                                VariableDeclaration(
                                        IdentifierName("var"))
                                    .WithVariables(
                                        SingletonSeparatedList(
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
                                                                    new SyntaxNodeOrToken[]
                                                                    {
                                                                        Argument(
                                                                            LiteralExpression(
                                                                                SyntaxKind
                                                                                    .NumericLiteralExpression,
                                                                                Literal(0))),
                                                                        Token(SyntaxKind.CommaToken),
                                                                        Argument(
                                                                            LiteralExpression(
                                                                                SyntaxKind
                                                                                    .NumericLiteralExpression,
                                                                                Literal(450)))
                                                                    }))),
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
                                            SingletonSeparatedList(
                                                Argument(
                                                    IdentifierName("output"))))))

                        ));
                                        
                
                return base.VisitMethodDeclaration(node);
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                // TODO: refactor to method
                if (!node.DescendantNodes().OfType<ConstructorDeclarationSyntax>().Any())
                    node = node.AddMembers(
                        ConstructorDeclaration(Identifier(node.Identifier.Text))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithBody(Block()));

                // TODO: refactor to method
                if (node.BaseList == null || !node.BaseList.DescendantNodes().OfType<SimpleBaseTypeSyntax>().Any(baseType => baseType.ToString().EndsWith("IDisposable")))
                    node = node.AddBaseListTypes(
                        SimpleBaseType(
                            QualifiedName(
                                IdentifierName("System"),
                                IdentifierName("IDisposable"))));

                // TODO: refactor to method
                if (node.DescendantNodes().OfType<MethodDeclarationSyntax>().All(method => method.Identifier.Text != "Dispose"))
                    node = node.AddMembers(
                        MethodDeclaration(
                                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                                Identifier("Dispose"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword))));
                
                return base.VisitClassDeclaration( 
                    node
                        .AddMembers(
                            FieldDeclaration(
                                VariableDeclaration(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("Xunit"),
                                                IdentifierName("Abstractions")),
                                            IdentifierName("ITestOutputHelper")))
                                    .WithVariables(
                                        SingletonSeparatedList(
                                            VariableDeclarator(
                                                Identifier("_testOutput"))))),
                            FieldDeclaration(
                                VariableDeclaration(
                                        QualifiedName(
                                            QualifiedName(IdentifierName("System"),
                                                IdentifierName("IO")),
                                            IdentifierName("StringWriter")))
                                    .WithVariables(
                                        SingletonSeparatedList(
                                            VariableDeclarator(
                                                Identifier("_stringWriter")))))));
            }
        }
    }
}