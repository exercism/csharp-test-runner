using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Exercism.TestRunner.CSharp
{
    internal class UnskipTestsRewriter : CSharpSyntaxRewriter
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
}