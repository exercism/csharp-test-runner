using System.Linq;
using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal class TestProject
    {
        public Project Project { get; }

        public TestProject(Project project) => Project = project;

        public Document TestsDocument() => Project.Documents.Single(document => document.Name.EndsWith("Test.cs"));
    }
}