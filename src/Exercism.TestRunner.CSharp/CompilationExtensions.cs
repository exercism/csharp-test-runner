using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal static class CompilationExtensions
    {
        public static bool HasErrors(this Compilation compilation) =>
            compilation.GetDiagnostics().Any(IsError);
        
        public static Diagnostic[] GetErrors(this Compilation compilation) =>
            compilation.GetDiagnostics().Where(IsError).ToArray();

        private static bool IsError(Diagnostic diagnostic) =>
            diagnostic.Severity == DiagnosticSeverity.Error;

        public static Assembly ToAssembly(this Compilation compilation)
        {
            using var memoryStream = new MemoryStream();
            var emitResult = compilation.Emit(memoryStream);
            return emitResult.Success ? Assembly.Load(memoryStream.ToArray()) : null;
        }
    }
}