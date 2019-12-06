using Humanizer;

namespace Exercism.TestRunner.CSharp
{
    internal static class SolutionParser
    {
        public static Solution Parse(Options options) =>
            new Solution(options.Slug, GetSolutionName(options));

        private static string GetSolutionName(Options options) =>
            options.Slug.Dehumanize().Pascalize();
    }
}