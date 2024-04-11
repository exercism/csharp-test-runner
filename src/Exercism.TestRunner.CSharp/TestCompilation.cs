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
        public static Compilation Compile(Options options, Files files) =>
            CSharpCompilation.Create(AssemblyName(), SyntaxTrees(options, files), References(), CompilationOptions());

        private static string AssemblyName() => Guid.NewGuid().ToString("N");

        private static IEnumerable<SyntaxTree> SyntaxTrees(Options options, Files files)
        {
            var solutionFiles = files.Solution.Select(file => ParseSyntaxTree(file, options));
            var editorFiles = files.Editor.Select(file => ParseSyntaxTree(file, options));
            var testFiles = files.Test.Select(file => ParseSyntaxTree(file, options).Rewrite());
            var additionalFiles = files.Additional.Select(file => ParseSyntaxTree(file, options));

            return solutionFiles.Concat(editorFiles).Concat(testFiles).Concat(additionalFiles);
        }

        private static SyntaxTree ParseSyntaxTree(string file, Options options)
        {
            var filePath = Path.Combine(options.InputDirectory, file);
            var source = SourceText.From(File.OpenRead(filePath));
            return CSharpSyntaxTree.ParseText(source, path: file);
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