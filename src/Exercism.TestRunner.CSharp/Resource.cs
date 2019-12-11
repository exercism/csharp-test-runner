using System.IO;

namespace Exercism.TestRunner.CSharp
{
    internal static class Resource
    {
        public static string Read(string fileName)
        {
            var resourceAssembly = typeof(Resource).Assembly;
            var resourceName = $"{typeof(Resource).Namespace}.Content.{fileName}";
            
            using var resource = resourceAssembly.GetManifestResourceStream(resourceName);
            using var streamReader = new StreamReader(resource);
            return streamReader.ReadToEnd();
        }
    }
}