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
            error.GetMessage();
    }
}