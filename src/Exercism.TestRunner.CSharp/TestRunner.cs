using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunner
    {
        public static async Task<TestRun> Run(Options options)
        {
            var compilation = await SolutionCompiler.Compile(options);
            var compilationWithAllTestsEnabled = compilation.EnableAllTests();

            var assembly = compilationWithAllTestsEnabled.ToAssembly();
            
            var diagnostics = compilationWithAllTestsEnabled.GetDiagnostics().ToArray();
            var errors = diagnostics.Where(diag => diag.Severity == DiagnosticSeverity.Error).ToArray();
            
            if (errors.Any())
                return new TestRun(message: string.Join("\n", errors.Select(error => error.GetMessage())), TestStatus.Error, Array.Empty<TestResult>());

            return await InMemoryXunitTestRunner.RunAllTests(assembly);
        }
    }
}