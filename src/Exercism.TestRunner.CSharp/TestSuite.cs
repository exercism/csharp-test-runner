using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Exercism.TestRunner.CSharp
{
    internal class TestSuite
    {
        private readonly SyntaxTree _originalSyntaxTree;
        private readonly string _testsFilePath;
        private readonly string _testResultsFilePath;

        private TestSuite(SyntaxTree originalSyntaxTree, string testsFilePath, string testResultsFilePath)
        {
            _originalSyntaxTree = originalSyntaxTree;
            _testsFilePath = testsFilePath;
            _testResultsFilePath = testResultsFilePath;
        }

        public void Run()
        {
            Rewrite();
            RunDotnetTest();
            UndoRewrite();
        }

        private void RunDotnetTest()
        {
            var command = "dotnet";
            var arguments = $"test --verbosity=quiet --logger \"trx;LogFileName={Path.GetFileName(_testResultsFilePath)}\" /flp:v=q";

            var processStartInfo = new ProcessStartInfo(command, arguments)
            {
                WorkingDirectory = Path.GetDirectoryName(_testsFilePath)!,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            Process.Start(processStartInfo)?.WaitForExit();
        }

        private void Rewrite() => File.WriteAllText(_testsFilePath, _originalSyntaxTree.Rewrite().ToString());

        private void UndoRewrite() => File.WriteAllText(_testsFilePath, _originalSyntaxTree.ToString());

        public static TestSuite FromOptions(Options options)
        {
            var originalSyntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(options.TestsFilePath));
            return new TestSuite(originalSyntaxTree, options.TestsFilePath, options.TestResultsFilePath);
        }
    }
}