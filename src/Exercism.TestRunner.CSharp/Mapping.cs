using System.Collections.Generic;
using System.Linq;

namespace Exercism.TestRunner.CSharp
{
    internal class Mapping
    {
        public Dictionary<string, string> PlaceholdersToIdentifier { get; }

        public Mapping(Dictionary<string, string> identifiersToPlaceholder) =>
            PlaceholdersToIdentifier = identifiersToPlaceholder.ToDictionary(kv => kv.Value, kv => kv.Key);
    }
}