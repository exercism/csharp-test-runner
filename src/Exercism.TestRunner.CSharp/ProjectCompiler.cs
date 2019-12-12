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
            var project = analyzer
                .AddToWorkspace(workspace)
                .AddAdditionalFile("TracingTestBase.cs")
                .AddAdditionalFile("TestOutputTraceListener.cs");

            // Buildalyzer issue not detecting output kind on macOS
            var compilation = await project.WithCompilationOptions(
                    project.CompilationOptions.WithOutputKind(OutputKind.DynamicallyLinkedLibrary))
                .GetCompilationAsync();

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
        
        private static Project AddAdditionalFile(this Project project, string fileName) =>
            project.AddDocument(fileName, AdditionalFile.Read(fileName)).Project;
    }
}