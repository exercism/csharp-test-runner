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
                .NormalizeWhitespace();

        private static SyntaxNode UnskipTests(this SyntaxNode testsRoot) =>
            new UnskipTestsRewriter().Visit(testsRoot);

        private class UnskipTestsRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitAttributeArgument(AttributeArgumentSyntax node) =>
                node.NameEquals?.Name.Identifier.Text == "Skip" ? null : base.VisitAttributeArgument(node);
        }
    }
}
