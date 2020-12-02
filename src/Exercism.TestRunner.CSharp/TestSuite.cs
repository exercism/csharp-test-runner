using System.Diagnostics;
using System.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Exercism.TestRunner.CSharp
{
    internal class TestSuite
    {
        private readonly SyntaxTree _originalSyntaxTree;
        private readonly Options _options;

        private TestSuite(SyntaxTree originalSyntaxTree, Options options)
        {
            _originalSyntaxTree = originalSyntaxTree;
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
            var command = "dotnet";
            var arguments = $"test --verbosity=quiet --logger \"trx;LogFileName={Path.GetFileName(_options.TestResultsFilePath)}\" /flp:v=q";

            var processStartInfo = new ProcessStartInfo(command, arguments)
            {
                WorkingDirectory = Path.GetDirectoryName(_options.TestsFilePath)!,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            Process.Start(processStartInfo)?.WaitForExit();
        }

        private void Rewrite() => File.WriteAllText(_options.TestsFilePath, _originalSyntaxTree.Rewrite().ToString());

        private void UndoRewrite() => File.WriteAllText(_options.TestsFilePath, _originalSyntaxTree.ToString());

        public static TestSuite FromOptions(Options options)
        {
            var originalSyntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(options.TestsFilePath));
            return new TestSuite(originalSyntaxTree, options);
        }
    }
}