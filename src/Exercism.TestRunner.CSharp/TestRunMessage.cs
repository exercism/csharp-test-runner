using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunMessage
    {
        public static string FromMessages(IEnumerable<string> messages) =>
            string.Join("\n", messages);

        public static string FromErrors(Diagnostic[] errors) =>
            string.Join("\n", errors.Select(FromError));

        private static string FromError(Diagnostic error) =>
            $"{System.IO.Path.GetFileName(error.Location.SourceTree.FilePath)}:{error.Location.GetLineSpan().StartLinePosition.Line}: {error.GetMessage()}";
    }
}