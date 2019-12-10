using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal static class CompilationExtensions
    {
        private static readonly RemoveSkipAttributeArgumentSyntaxRewriter RemoveSkipAttributeArgumentSyntaxRewriter = new RemoveSkipAttributeArgumentSyntaxRewriter();
        
        public static Compilation EnableAllTests(this Compilation compilation)
        {
            foreach (var syntaxTree in compilation.SyntaxTrees)
                compilation = compilation.ReplaceSyntaxTree(
                    syntaxTree,
                    syntaxTree.WithRootAndOptions(RemoveSkipAttributeArgumentSyntaxRewriter.Visit(syntaxTree.GetRoot()), syntaxTree.Options));

            return compilation;
        }

        public static Assembly ToAssembly(this Compilation compilation)
        {
            // TODO: using
            var memoryStream = new MemoryStream();
            var emitResult = compilation.Emit(memoryStream);
            return emitResult.Success ? Assembly.Load(memoryStream.ToArray()) : null;
        }
    }
}