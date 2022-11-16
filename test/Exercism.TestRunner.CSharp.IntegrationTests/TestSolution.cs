using System.IO;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    public class TestSolution
    {
        public string Slug { get; }
        public string Directory { get; }
        public string DirectoryFullPath => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(TestSolution).Assembly.Location)!, "tests", Directory));

        public TestSolution(string slug, string directory)
        {
            Slug = slug;
            Directory = directory;
        }
    }
}