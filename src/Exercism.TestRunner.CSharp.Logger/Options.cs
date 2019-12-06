namespace Microsoft.VisualStudio.TestPlatform.Extension.Exercism.TestLogger
{
    internal class Options
    {
        public string OutputDirectory { get; }

        public Options(string outputDirectory) => OutputDirectory = outputDirectory;
    }
}