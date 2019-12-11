using System.IO;

namespace Exercism.TestRunner.CSharp
{
    internal static class Resource
    {
        public static string Read(string fileName)
        {
            using var resource = typeof(Resource).Assembly.GetManifestResourceStream(fileName);
            using var streamReader = new StreamReader(resource);
            return streamReader.ReadToEnd();
        }
    }
}