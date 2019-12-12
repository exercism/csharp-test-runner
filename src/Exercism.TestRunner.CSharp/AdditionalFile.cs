using System.IO;

namespace Exercism.TestRunner.CSharp
{
    internal static class AdditionalFile
    {
        public static string Read(string fileName) =>
            File.ReadAllText(Path.Combine("AdditionalFiles", fileName));
    }
}