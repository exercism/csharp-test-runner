using System.Xml.Linq;

namespace Exercism.TestRunner.CSharp;

/// <summary>
/// Reduces a project file to a key that says how it compiles: its properties, Using items and package references.
/// Two project files with the same key compile the same way, apart from the project name.
/// The key is null for any project file that contains something the fast path does not know about,
/// such as conditions, imports, targets or other item types. Those projects always take the MSBuild path.
/// </summary>
internal static class ProjectShape
{
    // These properties do not change how the project compiles or runs. RootNamespace only names the
    // namespace of two generated files, so the fast path fills it in (see <see cref="RootNamespace"/>).
    private static readonly HashSet<string> IgnoredProperties = ["RestorePackagesWithLockFile", "IsPackable", "IsTestProject", "RootNamespace"];

    private static readonly Dictionary<string, string[]> AllowedItems = new()
    {
        ["PackageReference"] = ["Include", "Version"],
        ["Using"] = ["Include"]
    };

    // Metadata that a PackageReference may give as child elements. It becomes part of the key.
    private static readonly HashSet<string> AllowedPackageReferenceMetadata = ["IncludeAssets", "ExcludeAssets", "PrivateAssets"];

    public static string? Key(string projectFileText)
    {
        XElement root;
        try
        {
            root = XElement.Parse(projectFileText);
        }
        catch (System.Xml.XmlException)
        {
            return null;
        }

        if (root.Name != "Project" ||
            root.Attributes().Count() != 1 ||
            (string?)root.Attribute("Sdk") != "Microsoft.NET.Sdk")
            return null;

        var properties = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var items = new List<string>();

        foreach (var group in root.Elements())
        {
            if (group.HasAttributes || group.Name != "PropertyGroup" && group.Name != "ItemGroup")
                return null;

            foreach (var element in group.Elements())
            {
                if (group.Name == "PropertyGroup")
                {
                    if (element.HasAttributes || element.HasElements)
                        return null;
                    if (!IgnoredProperties.Contains(element.Name.LocalName))
                        properties[element.Name.LocalName] = element.Value.Trim();
                }
                else if (group.Name == "ItemGroup")
                {
                    if (!AllowedItems.TryGetValue(element.Name.LocalName, out var allowedAttributes) ||
                        element.Attributes().Any(attribute => !allowedAttributes.Contains(attribute.Name.LocalName)))
                        return null;
                    if (element.Elements().Any(metadata =>
                            element.Name != "PackageReference" ||
                            !AllowedPackageReferenceMetadata.Contains(metadata.Name.LocalName) ||
                            metadata.HasAttributes || metadata.HasElements))
                        return null;
                    items.Add(element.Name.LocalName + ":" + string.Join(";", element.Attributes()
                        .Select(attribute => $"{attribute.Name.LocalName}={attribute.Value}")
                        .Concat(element.Elements().Select(metadata => $"{metadata.Name.LocalName}={metadata.Value.Trim()}"))
                        .Order(StringComparer.Ordinal)));
                }
                else
                {
                    return null;
                }
            }
        }

        if (properties.Count == 0 || !IsValidRootNamespace(RootNamespaceProperty(root)))
            return null;

        items.Sort(StringComparer.Ordinal);
        return string.Join("\n", properties.Select(property => $"{property.Key}={property.Value}").Concat(items));
    }

    /// <summary>The project's RootNamespace property, or null when it does not set one.</summary>
    public static string? RootNamespace(string projectFileText) => RootNamespaceProperty(XElement.Parse(projectFileText));

    private static string? RootNamespaceProperty(XElement root) =>
        root.Elements("PropertyGroup").Elements("RootNamespace").LastOrDefault()?.Value.Trim();

    private static bool IsValidRootNamespace(string? rootNamespace) =>
        rootNamespace is null || System.Text.RegularExpressions.Regex.IsMatch(rootNamespace, @"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$");
}
