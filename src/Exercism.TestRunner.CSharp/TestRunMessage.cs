using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunMessage
    {
        public static string FromMessages(IEnumerable<string> messages) =>
            messages.Join();

        public static string FromErrors(Diagnostic[] errors) =>
            errors.Select(FromError).Join();

        private static string FromError(Diagnostic error) =>
            $"{System.IO.Path.GetFileName(error.Location.SourceTree.FilePath)}:{error.Location.GetLineSpan().StartLinePosition.Line}: {error.GetMessage()}";
    }
}