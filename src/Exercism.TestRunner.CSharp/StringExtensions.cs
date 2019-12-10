using System.Collections.Generic;

namespace Exercism.TestRunner.CSharp
{
    internal static class StringExtensions
    {
        public static string Join(this IEnumerable<string> messages) =>
            string.Join("\n", messages);

        public static string Normalize(this string str) =>
            str.Trim().Replace("\r\n", "\n");
    }
}