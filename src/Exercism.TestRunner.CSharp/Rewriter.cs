using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Exercism.TestRunner.CSharp;

internal static class Rewriter
{
    public const string AssemblyInfo =
        @"[assembly: CaptureConsole]
          [assembly: CaptureTrace]";

    internal static SyntaxTree RewriteTests(this SyntaxTree tree) =>
        tree.WithRootAndOptions(tree.GetRoot().RewriteTests(), tree.Options);

    private static SyntaxNode RewriteTests(this SyntaxNode node) =>
        node.UnskipTests().NormalizeWhitespace();

    private static SyntaxNode UnskipTests(this SyntaxNode testsRoot) =>
        new UnskipTestsRewriter().Visit(testsRoot);

    private sealed class UnskipTestsRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitAttributeArgument(AttributeArgumentSyntax node) =>
            node.NameEquals?.Name.Identifier.Text == "Skip" ? null : base.VisitAttributeArgument(node);
    }

    internal static string RewriteProjectFileText(string projectFileText) => projectFileText.Replace("net9.0", "net10.0");
}
