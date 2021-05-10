using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Exercism.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit.Runners;
using Xunit.Sdk;

using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

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

        public TestRun Run()
        {
            RunDotnetTest();

            return TestRunParser.Parse(_options, _compilation);
        }

        private void RunDotnetTest()
        {
            var emitResult = _compilation.Emit(_compilation.SourceModule.Name);
            Assembly.LoadFrom(_compilation.SourceModule.Name);

            var testResults = new List<TestResult>();
            
            var finished = new ManualResetEventSlim();
            var runner = AssemblyRunner.WithoutAppDomain(_compilation.SourceModule.Name);
            runner.OnTestFailed += info => testResults.Add(TestResultParser.FromTestInfo(info));
            runner.OnTestPassed += info => testResults.Add(TestResultParser.FromTestInfo(info));
            runner.OnExecutionComplete += _ => finished.Set();
            
            runner.Start();
            finished.Wait();


            // emitResult.Success
            // var command = "dotnet";
            // var arguments = $"test --verbosity=quiet --logger \"trx;LogFileName={Path.GetFileName(_options.TestResultsFilePath)}\" /flp:v=q";
            //
            // var processStartInfo = new ProcessStartInfo(command, arguments)
            // {
            //     WorkingDirectory = Path.GetDirectoryName(_options.TestsFilePath)!,
            //     RedirectStandardInput = true,
            //     RedirectStandardError = true,
            //     RedirectStandardOutput = true
            // };
            // Process.Start(processStartInfo)?.WaitForExit();
        }

        public static TestSuite FromOptions(Options options)
        {
            var syntaxTrees = Directory.EnumerateFiles(options.InputDirectory, "*.cs", SearchOption.AllDirectories)
                .Select(file =>
                {
                    var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(File.OpenRead(file)));
                    return file == options.TestsFilePath ? syntaxTree.Rewrite() : syntaxTree;
                })
                .ToArray();

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug, allowUnsafe: false);

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