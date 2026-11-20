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
    /// Writes the shape's key, and removes every file the fast path does not need, so that it stays out of the image.
    /// The fast path needs csc.rsp, shape.key, the files in obj/ named in csc.rsp, and the dependencies in publish/.
    /// </summary>
    public static int PrepareShape(string shapeDirectory)
    {
        var projectFile = Path.Combine(shapeDirectory, $"{Token}.csproj");
        var responseFile = Path.Combine(shapeDirectory, "csc.rsp");
        var key = ProjectShape.Key(File.ReadAllText(projectFile));
        if (key is null || !File.Exists(responseFile))
        {
            Console.Error.WriteLine($"Shape in {shapeDirectory} cannot be used by the fast path");
            return 1;
        }

        File.WriteAllText(Path.Combine(shapeDirectory, "shape.key"), key);
        foreach (var file in Directory.EnumerateFiles(shapeDirectory))
        {
            if (Path.GetFileName(file) is not ("csc.rsp" or "shape.key"))
                File.Delete(file);
        }
        Directory.Delete(Path.Combine(shapeDirectory, "bin"), recursive: true);
        foreach (var file in Directory.EnumerateFiles(Path.Combine(shapeDirectory, OutputSubdirectory), $"{Token}*"))
        {
            var name = Path.GetFileName(file);
            if (!name.EndsWith(".deps.json", StringComparison.Ordinal) && !name.EndsWith(".runtimeconfig.json", StringComparison.Ordinal))
                File.Delete(file);
        }

        // From obj/, the fast path reads only the generated source files and the analyzer config named in csc.rsp.
        // csc.rsp names each of them by its path relative to the project or by its full path.
        var responseFileText = File.ReadAllText(responseFile);
        var intermediateDirectory = Path.Combine(shapeDirectory, "obj");
        foreach (var file in Directory.EnumerateFiles(intermediateDirectory, "*", SearchOption.AllDirectories))
        {
            var isInput = Path.GetExtension(file) is ".cs" or ".editorconfig" &&
                          responseFileText.Contains(Path.GetRelativePath(shapeDirectory, file), StringComparison.Ordinal);
            if (!isInput)
                File.Delete(file);
        }
        foreach (var directory in Directory.EnumerateDirectories(intermediateDirectory, "*", SearchOption.AllDirectories).OrderByDescending(directory => directory.Length))
        {
            if (!Directory.EnumerateFileSystemEntries(directory).Any())
                Directory.Delete(directory);
        }

        return 0;
    }
}
