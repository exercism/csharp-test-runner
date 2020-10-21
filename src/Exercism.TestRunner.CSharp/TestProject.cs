using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Exercism.TestRunner.CSharp
{
    internal class TestProject
    {
        public Project Project { get; }
        public Options Options { get; }

        public TestProject(Project project, Options options)
        {
            Project = project;
            Options = options;
        }

        public Document TestsDocument() => Project.Documents.Single(document => document.Name.EndsWith("Test.cs"));
    }
    
    internal static class TestProjectReader
    {
        public static async Task<TestProject> FromOptions(Options options)
        {
            var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(options.ProjectFilePath());
            return new TestProject(project, options);
        }

        private static string ProjectFilePath(this Options options) =>
            Path.Combine(options.InputDirectory, $"{options.Exercise()}.csproj");

        private static string Exercise(this Options options) =>
            options.Slug.Dehumanize().Pascalize();
    }
}