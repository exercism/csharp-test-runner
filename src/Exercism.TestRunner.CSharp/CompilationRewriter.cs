using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Abstractions;
using Xunit.Sdk;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Exercism.TestRunner.CSharp
{
    internal static class CompilationRewriter
    {
        public static Compilation Rewrite(this Compilation compilation) =>
            compilation
                .UnskipTests()
                .CaptureTracesAsTestOutput();

        private static Compilation UnskipTests(this Compilation compilation) =>
            compilation.Rewrite(new UnskipTestsRewriter());

        private static Compilation CaptureTracesAsTestOutput(this Compilation compilation) =>
            compilation.Rewrite(new CaptureTracesAsTestOutputRewriter());

        private static Compilation Rewrite(this Compilation compilation, CSharpSyntaxRewriter rewriter)
        {
            foreach (var syntaxTree in compilation.SyntaxTrees)
                compilation = compilation.ReplaceSyntaxTree(
                    syntaxTree,
                    syntaxTree.WithRootAndOptions(
                        rewriter.Visit(syntaxTree.GetRoot()), syntaxTree.Options));

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

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.Expression is MemberAccessExpressionSyntax memberAccessExpression)
                {
                    var memberAccess = memberAccessExpression.WithoutTrivia().ToFullString();

                    if (memberAccess.EndsWith("Console.Write") ||
                        memberAccess.EndsWith("Console.Error.Write") ||
                        memberAccess.EndsWith("Console.Out.Write"))
                        return node.WithExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("System"),
                                        IdentifierName("Diagnostics")),
                                    IdentifierName("Trace")),
                                IdentifierName("Write")));

                    if (memberAccess.EndsWith("Console.WriteLine") ||
                        memberAccess.EndsWith("Console.Error.WriteLine") ||
                        memberAccess.EndsWith("Console.Out.WriteLine"))
                        return node.WithExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("System"),
                                        IdentifierName("Diagnostics")),
                                    IdentifierName("Trace")),
                                IdentifierName("WriteLine")));
                }

                return base.VisitInvocationExpression(node);
            }

            private static bool IsTestClass(SyntaxNode descendant) =>
                descendant is ClassDeclarationSyntax classDeclaration &&
                classDeclaration.Identifier.Text.EndsWith("Test");
        }
    }
}