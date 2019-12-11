using System.IO;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Humanizer;
using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal static class ProjectCompiler
    {
        public static async Task<Compilation> Compile(Options options)
        {
            AddDirectoryBuildProps(options);

            var manager = new AnalyzerManager();
            var analyzer = manager.GetProject(GetProjectPath(options));

            using var workspace = new AdhocWorkspace();
            var project = analyzer.AddToWorkspace(workspace);

            // TODO: report issue with Buildalyzer on Mac not detecting DLL output type
            project = project.WithCompilationOptions(
                project.CompilationOptions.WithOutputKind(OutputKind.DynamicallyLinkedLibrary));

            return await project.GetCompilationAsync();
        }

        private static string GetProjectPath(Options options) =>
            Path.Combine(options.InputDirectory, $"{options.Slug.Dehumanize().Pascalize()}.csproj");

        private static void AddDirectoryBuildProps(Options options)
        {
            const string directoryBuildPropsFileName = "Directory.Build.props";
            var directoryBuildPropsTemplate = Resource.Read(directoryBuildPropsFileName);
            var directoryBuildPropsPath = Path.Combine(options.InputDirectory, directoryBuildPropsFileName);
            var directoryBuildProps = directoryBuildPropsTemplate.Replace("$OutputDirectory", options.OutputDirectory);

            File.WriteAllText(directoryBuildPropsPath, directoryBuildProps);
        }
    }
}