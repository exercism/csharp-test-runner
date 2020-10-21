using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestProjectReader
    {
        public static async Task<TestProject> FromOptions(Options options)
        {
            var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(options.ProjectFilePath);
            return new TestProject(project, workspace);
        }
    }
}