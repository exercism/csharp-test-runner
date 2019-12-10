using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunMessage
    {
        public static string FromErrors(Diagnostic[] errors) =>
            Combine(errors.Select(FromError));

        private static string FromError(Diagnostic error) =>
            error.GetMessage();

        public static string FromFailedTest(ITestFailed testFailed) =>
            Combine(testFailed.Messages);
        
        private static string Combine(IEnumerable<string> messages) =>
            string.Join("\n", messages).Replace("\r\n", "\n");
    }
}