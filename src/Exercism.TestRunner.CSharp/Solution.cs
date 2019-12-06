namespace Exercism.TestRunner.CSharp
{
    internal class Solution
    {
        public string Name { get; }
        public string Slug { get; }

        public Solution(string slug, string name) =>
            (Slug, Name) = (slug, name);
    }
}