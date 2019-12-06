using System.IO;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Exercism.TestRunner.CSharp
{
    internal static class SolutionParser
    {
        public static Solution Parse(Options options) =>
            new Solution(options.Slug, GetSolutionName(options), GetImplementationDocument(options));

        private static string GetSolutionName(Options options) =>
            options.Slug.Dehumanize().Pascalize();

        private static Document GetImplementationDocument(Options options)
        {
            var implementationFile = GetImplementationFile(options);
            if (!implementationFile.Exists)
                return null;

            using (var fileStream = implementationFile.OpenRead())
            {
                var workspace = new AdhocWorkspace();
                var project = workspace.AddProject(implementationFile.DirectoryName, LanguageNames.CSharp);

                var sourceText = SourceText.From(fileStream);
                return project.AddDocument(implementationFile.Name, sourceText);
            }
        }

        private static FileInfo GetImplementationFile(Options options)
        {
            var implementationFileName = $"{GetSolutionName(options)}.cs";
            var implementationFilePath = Path.GetFullPath(Path.Combine(options.InputDirectory, implementationFileName));

            return new FileInfo(implementationFilePath);
        }
    }
}