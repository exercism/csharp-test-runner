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
            AddAdditionalFiles(options);

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

        private static void AddAdditionalFiles(Options options)
        {
            AddDirectoryBuildPropsFile(options);
            AddTestOutputTraceListenerFile(options);
            AddTracingTestFile(options);
        }

        private static void AddDirectoryBuildPropsFile(Options options)
        {
            var template = Resource.Read("Exercism.TestRunner.CSharp.AdditionalFiles.Directory.Build.props");
            var contents = template.Replace("$OutputDirectory", options.OutputDirectory);

            AddAdditionalFile("Directory.Build.props", contents, options);
        }

        private static void AddTestOutputTraceListenerFile(Options options) =>
            AddAdditionalFile("TestOutputTraceListener.cs", Resource.Read("TestOutputTraceListener"), options);

        private static void AddTracingTestFile(Options options) =>
            AddAdditionalFile("TracingTestBase.cs", Resource.Read("TracingTestBase"), options);

        private static void AddAdditionalFile(string fileName, string contents, Options options) =>
            File.WriteAllText(Path.Combine(options.InputDirectory, fileName), contents);
    }
}