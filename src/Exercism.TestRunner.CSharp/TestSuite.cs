using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Exercism.TestRunner.CSharp
{
    internal class TestSuite
    {
        private readonly SyntaxTree _originalSyntaxTree;
        private readonly string _originalProjectFile;
        private readonly Options _options;

        private TestSuite(SyntaxTree originalSyntaxTree, string originalProjectFile, Options options)
        {
            _originalSyntaxTree = originalSyntaxTree;
            _originalProjectFile = originalProjectFile;
            _options = options;
        }

        public TestRun Run()
        {
            Rewrite();
            RunDotnetTest();
            UndoRewrite();

            return TestRunParser.Parse(_options, _originalSyntaxTree);
        }

        private void RunDotnetTest()
        {
            var workingDirectory = Path.GetDirectoryName(_options.TestsFilePath)!;
            RunProcess("dotnet", "restore --source /root/.nuget/packages/", workingDirectory);
            RunProcess("dotnet", $"test --no-restore --verbosity=quiet --logger \"trx;LogFileName={Path.GetFileName(_options.TestResultsFilePath)}\" /flp:v=q", workingDirectory);            
        }

        private static void RunProcess(string command, string arguments, string workingDirectory)
        {
            var processStartInfo = new ProcessStartInfo(command, arguments)
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            Process.Start(processStartInfo)?.WaitForExit();
        }

        private void Rewrite()
        {
            RewriteProjectFile();
            RewriteTestsFile();
        }

        private void RewriteProjectFile() =>
            File.WriteAllText(_options.ProjectFilePath,
                _originalProjectFile
                    .Replace("net5.0", "net8.0")
                    .Replace("net6.0", "net8.0")
                    .Replace("net7.0", "net8.0")
                    .Replace("net8.0", "net9.0"));

        private void RewriteTestsFile() => File.WriteAllText(_options.TestsFilePath, _originalSyntaxTree.Rewrite().ToString());

        private void UndoRewrite()
        {
            UndoRewriteProjectFile();
            UndoRewriteTestsFile();
        }

        private void UndoRewriteProjectFile() => File.WriteAllText(_options.ProjectFilePath, _originalProjectFile);

        private void UndoRewriteTestsFile() => File.WriteAllText(_options.TestsFilePath, _originalSyntaxTree.ToString());

        public static TestSuite FromOptions(Options options)
        {
            var originalSyntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(options.TestsFilePath));
            var originalProjectFile = File.ReadAllText(options.ProjectFilePath);
            return new TestSuite(originalSyntaxTree, originalProjectFile, options);
        }
    }
}