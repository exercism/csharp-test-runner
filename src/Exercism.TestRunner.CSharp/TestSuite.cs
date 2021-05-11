using System.Linq;

using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestSuite
    {
        public static TestRun RunTests(Options options)
        {
            var files = FilesParser.Parse(options);
            var compilation = TestCompilation.Compile(options, files);

            var errors = compilation.GetDiagnostics()
                .Where(diag => diag.Severity == DiagnosticSeverity.Error)
                .ToArray();

            if (errors.Any())
                return TestRunParser.TestRunWithError(errors);

            var testResults = TestRunner.RunTests(compilation, files);
            return TestRunParser.TestRunWithoutError(testResults);
        }
    }
}