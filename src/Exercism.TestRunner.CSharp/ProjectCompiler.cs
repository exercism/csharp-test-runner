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
            CreateDirectoryBuildPropsFile(options);

            var manager = new AnalyzerManager();
            var analyzer = manager.GetProject(GetProjectPath(options));

            using var workspace = new AdhocWorkspace();
            var project = analyzer.AddToWorkspace(workspace);

            // TODO: report issue with Buildalyzer on Mac not detecting DLL output type
            project = project.WithCompilationOptions(
                project.CompilationOptions.WithOutputKind(OutputKind.DynamicallyLinkedLibrary));

            project = project.AddDocument("TracingTestBase.cs", AdditionalFile.Read("TracingTestBase.cs")).Project;
            project = project.AddDocument("TestOutputTraceListener.cs", AdditionalFile.Read("TestOutputTraceListener.cs")).Project;

            var compilation = await project.GetCompilationAsync();

            DeleteDirectoryBuildPropsFile(options);

            return compilation;
        }

        private static string GetProjectPath(Options options) =>
            Path.Combine(options.InputDirectory, $"{options.Slug.Dehumanize().Pascalize()}.csproj");

        private static void CreateDirectoryBuildPropsFile(Options options)
        {
            var template = AdditionalFile.Read("Directory.Build.props");
            var contents = template.Replace("$OutputDirectory", options.OutputDirectory);

            File.WriteAllText(GetDirectoryBuildPropsFilePath(options), contents);
        }

        private static void DeleteDirectoryBuildPropsFile(Options options) =>
            File.Delete(GetDirectoryBuildPropsFilePath(options));

        private static string GetDirectoryBuildPropsFilePath(Options options) =>
            Path.Combine(options.InputDirectory, "Directory.Build.props");
    }
}