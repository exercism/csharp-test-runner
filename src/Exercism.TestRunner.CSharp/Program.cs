using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Serilog;

namespace Exercism.TestRunner.CSharp
{
    public static class Program
    {   
        public static void Main(string[] args)
        {
            if (!MSBuildLocator.IsRegistered)
                MSBuildLocator.RegisterDefaults();
            
            Logging.Configure();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => CreateTestResults(options).GetAwaiter().GetResult());
        }

        private static async Task CreateTestResults(Options options)
        {
            Log.Information("Running test runner for {Exercise} solution in directory {Directory}", options.Slug, options.InputDirectory);

            var projectFilePath = @"/Users/erik/Code/exercism/csharp-test-runner/test/Exercism.TestRunner.CSharp.IntegrationTests/Solutions/MultipleTestsWithTestOutput/Fake.csproj";
            var projectDirectory = Path.GetDirectoryName(projectFilePath);

            using var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(projectFilePath);

            var testsDocument = project.Documents.Single(document => document.Name.EndsWith("Test.cs"));
            var testsRoot = await testsDocument.GetSyntaxRootAsync();
            testsDocument = testsDocument.WithSyntaxRoot(new UnskipTests().Visit(testsRoot));

            var tryApplyChanges = workspace.TryApplyChanges(testsDocument.Project.Solution);

            var processStartInfo = new ProcessStartInfo("dotnet", "test --verbosity=quiet --logger \"trx;LogFileName=tests.trx\" /flp:v=q");
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.WorkingDirectory = projectDirectory;
            Process.Start(processStartInfo)?.WaitForExit();

            var logLines = File.ReadLines(Path.Combine(projectDirectory, "msbuild.log"));
            


            // var testRun = await TestRunner.Run(options);
            // TestRunWriter.WriteToFile(options, testRun);

            Log.Information("Ran test runner for {Exercise} solution in directory {Directory}", options.Slug, options.OutputDirectory);
        }
    }

    class UnskipTests : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitAttribute(AttributeSyntax node)
        {
            if (node.Name.ToString() == "Fact")
            {
                return base.VisitAttribute(node.WithArgumentList(null));
            }

            return base.VisitAttribute(node);
        }
    }
}