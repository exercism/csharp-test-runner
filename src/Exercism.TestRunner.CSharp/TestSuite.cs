using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Exercism.TestRunner.CSharp
{
    internal class TestSuite
    {
        private readonly string _testsFilePath;
        private readonly string _testResultsFilePath;

        private TestSuite(string testsFilePath, string testResultsFilePath)
        {
            _testsFilePath = testsFilePath;
            _testResultsFilePath = testResultsFilePath;
        }

        public void Run()
        {
            Rewrite();
            RunDotnetTest();
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

        private void Rewrite()
        {
            using var fileStream = File.Open(_testsFilePath, FileMode.Open, FileAccess.ReadWrite);
            var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(fileStream));

            var rewrittenSyntaxTree = syntaxTree.Rewrite();
            var sourceText = rewrittenSyntaxTree.GetText();
            
            using var fileWriter = new StreamWriter(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);
            sourceText.Write(fileWriter);
        }
        
        public static TestSuite FromOptions(Options options) => new TestSuite(options.TestsFilePath, options.TestResultsFilePath);
    }
}