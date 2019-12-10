using System.IO;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Humanizer;
using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal static class SolutionCompiler
    {
        public static async Task<Compilation> Compile(Options options)
        {
            var manager = new AnalyzerManager();
            var analyzer = manager.GetProject(GetProjectPath(options));

            using var workspace = new AdhocWorkspace();
            var project = analyzer.AddToWorkspace(workspace);

            return await project.GetCompilationAsync();
        }

        private static string GetProjectPath(Options options) =>
            Path.Combine(options.InputDirectory, $"{options.Slug.Dehumanize().Pascalize()}.csproj");
    }
}