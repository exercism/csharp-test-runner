using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Exercism.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit.Runners;

namespace Exercism.TestRunner.CSharp
{
    internal class TestSuite
    {
        private readonly Compilation _compilation;
        private readonly Options _options;

        private TestSuite(Compilation compilation, Options options)
        {
            _compilation = compilation;
            _options = options;
        }

        public TestRun RunTests()
        {
            var errors = _compilation.GetDiagnostics()
                .Where(diag => diag.Severity == DiagnosticSeverity.Error)
                .ToArray();

            if (errors.Any())
            {
                return TestRunParser.TestRunWithError(errors);
            }

            var testResults = RunDotnetTest();
            return TestRunParser.TestRunWithoutError(testResults);
        }

        private TestResult[] RunDotnetTest()
        {
            var outputPath = Path.Combine(Path.GetTempPath(), _compilation.SourceModule.Name);
            _compilation.Emit(outputPath);
            Assembly.LoadFrom(outputPath);

            var testsSyntaxTree = _compilation.SyntaxTrees.First(tree => tree.FilePath == _options.TestsFilePath);

            var passedTests = new List<TestPassedInfo>();
            var failedTests = new List<TestFailedInfo>();
            
            var finished = new ManualResetEventSlim();
            var runner = AssemblyRunner.WithoutAppDomain(outputPath);
            runner.OnTestFailed += info => failedTests.Add(info);
            runner.OnTestPassed += info => passedTests.Add(info);
            runner.OnExecutionComplete += _ => finished.Set();
            
            // TODO: sort tests
            
            runner.Start();
            finished.Wait();

            return TestResultParser.FromTests(passedTests, failedTests, testsSyntaxTree);
        }

        public static TestSuite FromOptions(Options options)
        {
            SyntaxTree ParseSyntaxTree(string file)
            {
                var source = SourceText.From(File.OpenRead(file));
                var syntaxTree = CSharpSyntaxTree.ParseText(source, path: file);
                
                // We need to rewrite the test suite to un-skip all tests as well
                // as capture any console output
                if (file == options.TestsFilePath)
                {
                    return syntaxTree.Rewrite();
                }

                return syntaxTree;
            }

            var syntaxTrees = Directory.EnumerateFiles(options.InputDirectory, "*.cs", SearchOption.AllDirectories)
                .Select(ParseSyntaxTree)
                .ToArray();

            // TODO: check time difference between release and debug
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release);
            var trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!.Split(Path.PathSeparator);
            var references = trustedAssembliesPaths
                .Select(p => MetadataReference.CreateFromFile(p))
                .Append(MetadataReference.CreateFromFile(typeof(Xunit.FactAttribute).Assembly.Location))
                .Append(MetadataReference.CreateFromFile(typeof(Xunit.Assert).Assembly.Location))
                .Append(MetadataReference.CreateFromFile(typeof(TaskAttribute).Assembly.Location))
                .ToList();
            
            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString("N"),  syntaxTrees, references, compilationOptions);
            return new TestSuite(compilation, options);
        }
    }
}