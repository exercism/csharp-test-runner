using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Exercism.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestCompilation
    {
        public static Compilation Compile(Options options) =>
            CSharpCompilation.Create(Guid.NewGuid().ToString("N"), SyntaxTrees(options), References(), CompilationOptions());

        private static IEnumerable<SyntaxTree> SyntaxTrees(Options options)
        {
            SyntaxTree ParseSyntaxTree(string file)
            {
                var source = SourceText.From(File.OpenRead(file));
                var syntaxTree = CSharpSyntaxTree.ParseText(source, path: file);

                // We need to rewrite the test suite to un-skip all tests and capture any console output
                if (file == options.TestsFilePath)
                {
                    return syntaxTree.Rewrite();
                }

                return syntaxTree;
            }

            return Directory.EnumerateFiles(options.InputDirectory, "*.cs", SearchOption.AllDirectories)
                .Select(ParseSyntaxTree);
        }

        private static CSharpCompilationOptions CompilationOptions() =>
            new(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release);

        private static IEnumerable<PortableExecutableReference> References()
        {
            var trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!.Split(Path.PathSeparator);
            return trustedAssembliesPaths
                .Select(p => MetadataReference.CreateFromFile(p))
                .Append(MetadataReference.CreateFromFile(typeof(Xunit.FactAttribute).Assembly.Location))
                .Append(MetadataReference.CreateFromFile(typeof(Xunit.Assert).Assembly.Location))
                .Append(MetadataReference.CreateFromFile(typeof(TaskAttribute).Assembly.Location));
        }
    }
}