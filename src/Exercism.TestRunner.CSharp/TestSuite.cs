using System.Linq;

using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestSuite
    {
        public static TestRun RunTests(Options options)
        {
            var compilation = TestCompilation.Compile(options);

            var errors = compilation.GetDiagnostics()
                .Where(diag => diag.Severity == DiagnosticSeverity.Error)
                .ToArray();

            if (errors.Any())
                return TestRunParser.TestRunWithError(errors);

            var testResults = TestRunner.RunTests(compilation, options);
            return TestRunParser.TestRunWithoutError(testResults);
        }
    }
}