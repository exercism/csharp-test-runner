using System.IO;
using System.Reflection;

namespace Exercism.TestRunner.CSharp
{
    internal static class AdditionalFile
    {
        public static string Read(string fileName)
        {
            var resourceFilePath = $"{typeof(AdditionalFile).Namespace}.AdditionalFiles.{fileName}";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceFilePath);
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}