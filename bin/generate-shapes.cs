#!/usr/bin/env dotnet

// File-based apps turn on native AOT by default, which makes restore download the AOT compiler and
// runtime packs. This app only runs with `dotnet run`, so turning AOT off lets it run offline.
#:property PublishAot=false
#:package LibGit2Sharp@0.32.0

// Writes the numbered folders in shapes/ from the history of the exercism/csharp track repo.
//
// A shape is a distinct xUnit v3 project file, ignoring the project name and the settings that do not
// affect compiling. This program reads every exercise's .csproj in every commit since the track switched
// to xUnit v3, groups them by shape, and writes one representative project file per shape to
// <out_dir>/<n>/ExercismShape.csproj, with net9.0 replaced by net10.0 as the runner does. The Dockerfile
// builds each one (bin/build-shapes.sh), so that the fast path can reuse its compiler arguments.
//
// The grouping here is close to ProjectShape.Key in the runner. If the two ever disagree, the worst
// case will be a duplicate template, or a project file with no matching template, which uses MSBuild.
//
// This is a file-based app, so it needs no project file. Usage, from the repo root:
//   bin/generate-shapes.cs shapes [path to the exercism/csharp repo, default ../csharp]

using System.Text;
using System.Xml.Linq;
using LibGit2Sharp;

if (args.Length is < 1 or > 2)
{
    Console.Error.WriteLine("usage: bin/generate-shapes.cs <out_dir> [path to the exercism/csharp repo]");
    return 1;
}

var outDirectory = args[0];
var trackDirectory = args.Length > 1 ? args[1] : "../csharp";
HashSet<string> ignoredProperties = ["RestorePackagesWithLockFile", "IsPackable", "IsTestProject"];

// Each shape keeps the first project file found for it, and the exercises that use it.
// The list keeps the shapes in the order they were first found.
var shapes = new List<(string Key, string Text, HashSet<string> Exercises)>();
var shapesByKey = new Dictionary<string, int>();

using (var repository = new Repository(trackDirectory))
{
    // Read the commits since the track switched to xUnit v3, newest first. Commits that leave the
    // exercises folder as it was would find the same files again, so they are skipped.
    var since = new DateTimeOffset(new DateTime(2025, 2, 24));
    var commits = repository.Commits
        .QueryBy(new CommitFilter { SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time })
        .TakeWhile(commit => commit.Committer.When >= since)
        .DistinctBy(commit => commit["exercises"].Target.Id);

    foreach (var commit in commits)
    {
        foreach (var (path, blob) in ProjectFiles((Tree)commit["exercises"].Target, "exercises"))
        {
            var text = Text(blob);
            if (!text.Contains("xunit.v3", StringComparison.Ordinal))
                continue;

            var key = Key(text);
            if (!shapesByKey.TryGetValue(key, out var index))
            {
                index = shapes.Count;
                shapesByKey[key] = index;
                shapes.Add((key, text, []));
            }
            shapes[index].Exercises.Add(path.Split('/')[2]);
        }
    }
}

// Replace only the numbered folders, so that the shared files in the output folder stay.
if (Directory.Exists(outDirectory))
{
    foreach (var directory in Directory.EnumerateDirectories(outDirectory).Where(directory => Path.GetFileName(directory).All(char.IsAsciiDigit)))
        Directory.Delete(directory, recursive: true);
}

// OrderBy is a stable sort, so shapes used by the same number of exercises stay in the order found.
var ordered = shapes.OrderByDescending(shape => shape.Exercises.Count).ToArray();
for (var i = 0; i < ordered.Length; i++)
{
    var directory = Path.Combine(outDirectory, i.ToString("00"));
    Directory.CreateDirectory(directory);
    File.WriteAllText(Path.Combine(directory, "ExercismShape.csproj"), ordered[i].Text.Replace("net9.0", "net10.0"), new UTF8Encoding(false));
    Console.WriteLine($"{i} {ordered[i].Exercises.Count} {string.Join(", ", ordered[i].Exercises.Order(StringComparer.Ordinal).Take(3))}");
}

return 0;

// Reduces a project file to its properties and items, sorted, ignoring the properties that do not
// affect compiling.
string Key(string text)
{
    var root = XElement.Parse(text.Replace("net9.0", "net10.0").TrimStart('﻿'));
    var properties = new SortedDictionary<string, string>(StringComparer.Ordinal);
    var items = new List<string>();
    foreach (var group in root.Elements())
    {
        foreach (var element in group.Elements())
        {
            if (group.Name == "PropertyGroup")
            {
                if (!ignoredProperties.Contains(element.Name.ToString()))
                    properties[element.Name.ToString()] = LeadingText(element);
            }
            else
            {
                var attributes = element.Attributes().Select(attribute => $"{attribute.Name}={attribute.Value}").Order(StringComparer.Ordinal);
                var metadata = element.Elements().Select(child => $"{child.Name}={LeadingText(child)}").Order(StringComparer.Ordinal);
                items.Add($"{element.Name}({string.Join("\u0001", attributes.Concat(metadata))})");
            }
        }
    }

    items.Sort(StringComparer.Ordinal);
    return string.Join("\u0002", properties.Select(property => $"{property.Key}={property.Value}")) + "\u0003" + string.Join("\u0002", items);
}

// The element's text before its first child element, trimmed.
static string LeadingText(XElement element) =>
    string.Concat(element.Nodes().TakeWhile(node => node is not XElement).OfType<XText>().Select(text => text.Value)).Trim();

// Every .csproj file in the tree and its subfolders, in the order `git ls-tree -r` lists them.
static IEnumerable<(string Path, Blob Blob)> ProjectFiles(Tree tree, string path)
{
    foreach (var entry in tree)
    {
        var entryPath = $"{path}/{entry.Name}";
        if (entry.Target is Tree subtree)
        {
            foreach (var file in ProjectFiles(subtree, entryPath))
                yield return file;
        }
        else if (entry.Target is Blob blob && entry.Name.EndsWith(".csproj", StringComparison.Ordinal))
            yield return (entryPath, blob);
    }
}

// Returns the file as text, with Windows and old Mac line endings turned into \n. A byte order
// mark at the start stays in the text, so that it is written back out with the file.
static string Text(Blob blob)
{
    using var stream = blob.GetContentStream();
    using var memory = new MemoryStream();
    stream.CopyTo(memory);
    var text = new UTF8Encoding(false, true).GetString(memory.ToArray());
    return text.Replace("\r\n", "\n").Replace('\r', '\n');
}
