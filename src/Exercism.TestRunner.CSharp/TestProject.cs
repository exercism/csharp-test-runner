using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Exercism.TestRunner.CSharp
{
    internal class TestProject
    {
        public Project Project { get; }
        public MSBuildWorkspace Workspace { get; }

        public TestProject(Project project, MSBuildWorkspace workspace)
        {
            Project = project;
            Workspace = workspace;
        }

        public Document TestsDocument() => Project.Documents.Single(document => document.Name.EndsWith("Test.cs"));
    }
}