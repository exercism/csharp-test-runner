using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Exercism.TestRunner.CSharp
{
    internal class RemoveSkipAttributeArgumentSyntaxRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitAttributeArgument(AttributeArgumentSyntax node) =>
            AttributeArgumentNameMatches(node) ? null : base.VisitAttributeArgument(node);
     
        private static bool AttributeArgumentNameMatches(AttributeArgumentSyntax node) =>
            node.NameEquals?.Name.ToString() == "Skip";
    }
}