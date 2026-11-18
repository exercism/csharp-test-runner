namespace Exercism.TestRunner.CSharp;

/// <summary>
/// Prepares the template project for each project shape at image build (see bin/build-shapes.sh).
/// The shapes are for a fast path, which will compile a solution with Roslyn inside this process,
/// using the compiler arguments and generated files kept from its shape's build.
/// </summary>
internal static class FastPath
{
    // The template project's name.
    private const string Token = "ExercismShape";
    // The template's ReadyToRun publish folder, which holds every dependency.
    private const string OutputSubdirectory = "publish";

    /// <summary>
    /// Runs at image build, after the real SDK has built the shape's template project in <paramref name="shapeDirectory"/>.
    /// Writes the shape's key and removes the template's own compiled files.
    /// </summary>
    public static int PrepareShape(string shapeDirectory)
    {
        var projectFile = Path.Combine(shapeDirectory, $"{Token}.csproj");
        var key = ProjectShape.Key(File.ReadAllText(projectFile));
        if (key is null || !File.Exists(Path.Combine(shapeDirectory, "csc.rsp")))
        {
            Console.Error.WriteLine($"Shape in {shapeDirectory} cannot be used by the fast path");
            return 1;
        }

        File.WriteAllText(Path.Combine(shapeDirectory, "shape.key"), key);
        Directory.Delete(Path.Combine(shapeDirectory, "bin"), recursive: true);
        foreach (var file in Directory.EnumerateFiles(Path.Combine(shapeDirectory, OutputSubdirectory), $"{Token}*"))
        {
            var name = Path.GetFileName(file);
            if (!name.EndsWith(".deps.json", StringComparison.Ordinal) && !name.EndsWith(".runtimeconfig.json", StringComparison.Ordinal))
                File.Delete(file);
        }

        return 0;
    }
}
