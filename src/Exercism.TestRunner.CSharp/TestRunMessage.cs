using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunMessage
    {
        public static string FromMessages(IEnumerable<string> messages) =>
            Combine(messages);

        public static string FromErrors(Diagnostic[] errors) =>
            Combine(errors.Select(FromError));

        private static string FromError(Diagnostic error) =>
            error.GetMessage();
        
        private static string Combine(IEnumerable<string> messages) =>
            string.Join("\n", messages).Replace("\r\n", "\n");
    }
}