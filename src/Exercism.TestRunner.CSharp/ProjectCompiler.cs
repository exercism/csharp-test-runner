using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

namespace Exercism.TestRunner.CSharp
{
    internal static class ProjectCompiler
    {
        public static async Task<Compilation> Compile(Options options)
        {
            CreateDirectoryBuildPropsFile(options);

            var workspace = MSBuildWorkspace.Create();
            var project = (await workspace.OpenProjectAsync(GetProjectPath(options)))
                .AddAdditionalFile("TracingTestBase.cs")
                .AddAdditionalFile("TestOutputTraceListener.cs")
                .WithMetadataReferences(GetMetadataReferences())
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                
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
        
        private static Project AddAdditionalFile(this Project project, string fileName) =>
            project.AddDocument(fileName, AdditionalFile.Read(fileName)).Project;
        
        private static IEnumerable<PortableExecutableReference> GetMetadataReferences() =>
            AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
                .ToString()
                .Split(":")
                .Select(metadataFilePath => MetadataReference.CreateFromFile(metadataFilePath));
    }
}