using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace Exercism.TestRunner.CSharp;

/// <summary>
/// Compiles the solution with Roslyn inside this process and runs the xUnit v3 test executable,
/// skipping MSBuild.
///
/// Why it is faster: `dotnet restore` and `dotnet build` start MSBuild, evaluate the project, resolve
/// packages and start the compiler server. Most of that work gives the same answer for every solution
/// of an exercise. Here we do only the parts that depend on the solution:
/// compile its files and run its tests.
///
/// How the project file is still respected: the shapes/ folder holds one template project file for each
/// distinct project file in the track (a "shape"). At image build, the real SDK builds each template
/// (bin/build-shapes.sh), and we keep its exact compiler command line (csc.rsp), its generated source files
/// (such as the global usings, assembly attributes and the test entry point) and a ReadyToRun-precompiled
/// output folder with every dependency (see <see cref="PrepareShape"/>). At run time,
/// <see cref="ProjectShape.Key"/> reduces the solution's .csproj to the settings that affect compiling,
/// and we only go ahead if a template has exactly the same key. The compile then uses the arguments
/// produced by MSBuild for an identical .csproj.
///
/// Why the results stay the same: compile errors are formatted as MSBuild writes them to msbuild.log, and
/// the test executable's report is read by the same code as on the MSBuild path (<see cref="TestResultParser"/>).
///
/// The fast path never writes into the solution folder. It returns null whenever it meets something it
/// does not fully understand, and the caller then runs the tests with <see cref="MSBuildTestrunner"/>.
/// </summary>
internal sealed partial class InMemoryCompiledTestRunner(Options options, SyntaxTree originalSyntaxTree, string originalProjectFile) : ITestRunner
{
    public const string ShapesDirectory = "/opt/test-runner/shapes";
    /// <summary>
    /// The template project's name. We replace it with the solution's project name wherever it appears.
    /// </summary>
    private const string Token = "ExercismShape";

    /// <summary>
    /// The template's ReadyToRun publish folder, which holds every dependency.
    /// </summary>
    private const string OutputSubdirectory = "publish";

    private const string IntermediateSubdirectory = "obj/release/net10.0";

    /// <summary>
    /// Files that could change how MSBuild builds the project. If the solution folder has any of them,
    /// the fast path will not run.
    /// </summary>
    private static readonly string[] DisallowedFileNames =
    [
        "directory.build.props", "directory.build.targets", "directory.packages.props", "directory.solution.props",
        "global.json", "nuget.config", "assemblyinfo.cs"
    ];

    private static readonly string[] DisallowedExtensions =
        [".props", ".targets", ".sln", ".slnx", ".resx", ".globalconfig", ".csproj", ".fsproj", ".vbproj", ".proj"];

    /// <summary>
    /// The project file as MSBuild would build it, after <see cref="MSBuildTestrunner"/> rewrites it.
    /// </summary>
    private readonly string projectFileText = MSBuildTestrunner.RewriteProjectFileText(originalProjectFile);

    private string ProjectName => Path.GetFileNameWithoutExtension(options.ProjectFilePath);

    /// <summary>
    /// The project's root namespace. MSBuild uses the project name when the project file sets no RootNamespace.
    /// </summary>
    private string RootNamespace => ProjectShape.RootNamespace(projectFileText) ?? ProjectName;

    private string SolutionDirectory => Path.GetDirectoryName(Path.GetFullPath(options.ProjectFilePath))!;

    /// <summary>
    /// Compiles the solution and runs its tests without MSBuild. Before every return of null, it logs the reason
    /// to the console.
    /// </summary>
    /// <returns>The test run, or null if the caller should use the MSBuild path instead.</returns>
    public TestRun? Run()
    {
        var shapeDirectory = FindShapeDirectory();
        if (shapeDirectory is null)
        {
            Console.WriteLine("Fast path not used: unknown project shape");
            return null;
        }

        var sourceFiles = CollectSourceFiles();
        if (sourceFiles is null)
        {
            Console.WriteLine("Fast path not used: unusual files in the solution folder");
            return null;
        }

        var compilation = CreateCompilation(shapeDirectory, sourceFiles);
        if (compilation is null)
        {
            Console.WriteLine("Fast path not used: could not prepare the compilation");
            return null;
        }

        var errors = CompileErrors(compilation);
        if (errors.Length > 0)
            return CompileErrorTestRun(errors);

        var runDirectory = PrepareRunDirectory(shapeDirectory);
        var emitResult = Emit(compilation, runDirectory);
        if (!emitResult.Success)
        {
            var emitErrors = emitResult.Diagnostics.Where(IsError).ToArray();
            if (emitErrors.Length > 0)
                return CompileErrorTestRun(emitErrors);

            Console.WriteLine("Fast path not used: emit failed without errors");
            return null;
        }

        Console.WriteLine("Fast path used: tests run");
        return RunTests(runDirectory);
    }

    private string? FindShapeDirectory()
    {
        var key = ProjectShape.Key(projectFileText);
        if (key is null || !Directory.Exists(ShapesDirectory))
            return null;

        foreach (var directory in Directory.EnumerateDirectories(ShapesDirectory))
        {
            var keyFile = Path.Combine(directory, "shape.key");
            if (File.Exists(keyFile) && File.ReadAllText(keyFile) == key)
                return directory;
        }

        return null;
    }

    /// <summary>
    /// Lists the C# files that MSBuild's default Compile glob would include (**/*.cs, apart from bin/, obj/
    /// and folders that start with a dot), in the order it would find them.
    /// </summary>
    /// <returns>The source files, or null if the folder holds anything that could make MSBuild compile differently.</returns>
    private List<string>? CollectSourceFiles()
    {
        var root = SolutionDirectory;
        if (Directory.Exists(Path.Combine(root, "bin")) || Directory.Exists(Path.Combine(root, "obj")))
            return null;

        var files = new List<string>();
        return CollectSourceFiles(root, files) ? files : null;
    }

    private bool CollectSourceFiles(string directory, List<string> files)
    {
        foreach (var file in Directory.EnumerateFiles(directory))
        {
            var name = Path.GetFileName(file);
            var lowerName = name.ToLowerInvariant();
            if (DisallowedFileNames.Contains(lowerName))
                return false;
            if (DisallowedExtensions.Contains(Path.GetExtension(lowerName)) && Path.GetFullPath(file) != Path.GetFullPath(options.ProjectFilePath))
                return false;
            if (lowerName == ".editorconfig" && EditorConfigChangesDiagnostics(file))
                return false;
            if (name.EndsWith(".cs", StringComparison.Ordinal))
                files.Add(file);
        }

        foreach (var subdirectory in Directory.EnumerateDirectories(directory))
        {
            if (Path.GetFileName(subdirectory).StartsWith('.'))
                continue;
            if (!CollectSourceFiles(subdirectory, files))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks whether an .editorconfig could change the severity of compiler diagnostics, which the fast path
    /// does not apply.
    /// </summary>
    /// <returns>True if the file has any setting that could change diagnostics.</returns>
    private static bool EditorConfigChangesDiagnostics(string path)
    {
        var text = File.ReadAllText(path);
        return text.Contains("dotnet_diagnostic", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("dotnet_analyzer_diagnostic", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("is_global", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("build_property", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("generated_code", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The fast path does not run source generators or analyzers. This matches code that needs a generator, and
    /// test attributes outside the test file (which the xUnit analyzers check), so that they take the MSBuild path.
    /// </summary>
    [GeneratedRegex(@"\b(GeneratedRegex|LibraryImport|JsonSerializable|GeneratedComInterface|GeneratedComClass|JSImport|JSExport|LoggerMessage|Fact|Theory)\b")]
    private static partial Regex NeedsGeneratorOrAnalyzer();

    private CSharpCompilation? CreateCompilation(string shapeDirectory, List<string> sourceFiles)
    {
        var responseFile = Path.Combine(shapeDirectory, "csc.rsp");
        var arguments = File.ReadAllLines(responseFile)
            .SelectMany(line => CommandLineParser.SplitCommandLineIntoArguments(line, removeHashComments: false))
            .ToArray();
        var commandLine = CSharpCommandLineParser.Default.Parse(arguments, shapeDirectory, sdkDirectory: null);
        if (commandLine.Errors.Any(IsError))
            return null;

        var parseOptions = commandLine.ParseOptions;
        var trees = new List<SyntaxTree>();
        var testsFilePath = Path.GetFullPath(options.TestsFilePath);

        foreach (var file in sourceFiles)
        {
            var fullPath = Path.GetFullPath(file);
            if (fullPath == testsFilePath)
            {
                trees.Add(CSharpSyntaxTree.ParseText(SourceText.From(originalSyntaxTree.Rewrite().ToString(), Encoding.UTF8), parseOptions, fullPath));
                continue;
            }

            var bytes = File.ReadAllBytes(fullPath);
            if (!IsValidUtf8(bytes))
                return null;
            var text = SourceText.From(bytes, bytes.Length, Encoding.UTF8, canBeEmbedded: true);
            if (NeedsGeneratorOrAnalyzer().IsMatch(text.ToString()))
                return null;
            trees.Add(CSharpSyntaxTree.ParseText(text, parseOptions, fullPath));
        }

        trees.Add(CSharpSyntaxTree.ParseText(SourceText.From(MSBuildTestrunner.AssemblyInfo, Encoding.UTF8), parseOptions, Path.Combine(SolutionDirectory, "AssemblyInfo.cs")));

        // The template's two placeholder files (in the shapes folder itself) are on the command line too, and we
        // leave them out. The rest are generated files, which we move to where MSBuild would have put them for
        // this project and rename.
        var templateFiles = new[] { $"{Token}.cs", $"{Token}Tests.cs" }.Select(name => Path.Combine(ShapesDirectory, name)).ToHashSet();
        foreach (var sourceFile in commandLine.SourceFiles)
        {
            if (templateFiles.Contains(sourceFile.Path))
                continue;

            var text = File.ReadAllText(sourceFile.Path)
                .Replace($"namespace {Token}", $"namespace {RootNamespace}")
                .Replace(Token, ProjectName);
            var path = sourceFile.Path.StartsWith(shapeDirectory + "/", StringComparison.Ordinal)
                ? Path.Combine(SolutionDirectory, Path.GetRelativePath(shapeDirectory, sourceFile.Path)).Replace(Token, ProjectName)
                : sourceFile.Path;
            trees.Add(CSharpSyntaxTree.ParseText(SourceText.From(text, Encoding.UTF8), parseOptions, path));
        }

        var references = commandLine.MetadataReferences
            .Select(reference => (MetadataReference)MetadataReference.CreateFromFile(reference.Reference, reference.Properties))
            .ToArray();

        var compilationOptions = commandLine.CompilationOptions
            .WithModuleName($"{ProjectName}.dll")
            .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default)
            .WithSourceReferenceResolver(SourceFileResolver.Default)
            .WithMetadataReferenceResolver(null)
            .WithXmlReferenceResolver(null)
            .WithStrongNameProvider(new DesktopStrongNameProvider());

        return CSharpCompilation.Create(ProjectName, trees, references, compilationOptions);
    }

    private static bool IsValidUtf8(byte[] bytes)
    {
        try
        {
            _ = new UTF8Encoding(false, true).GetString(bytes);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    /// <summary>
    /// Finds the errors that csc would report before emitting. csc stops after the first stage that has errors:
    /// parsing, then declarations. Method body errors are found while emitting.
    /// </summary>
    /// <returns>The errors from the first stage that has any, or an empty array.</returns>
    private static Diagnostic[] CompileErrors(CSharpCompilation compilation)
    {
        var parseErrors = compilation.GetParseDiagnostics().Where(IsError).ToArray();
        if (parseErrors.Length > 0)
            return parseErrors;

        return compilation.GetDeclarationDiagnostics().Where(IsError).ToArray();
    }

    private static bool IsError(Diagnostic diagnostic) => diagnostic.Severity == DiagnosticSeverity.Error;

    private TestRun CompileErrorTestRun(IEnumerable<Diagnostic> errors)
    {
        Console.WriteLine("Fast path used: compile errors");
        // The same lines that MSBuild writes to msbuild.log, which the normal path then reads.
        var logLines = errors.Select(FormatAsMsBuildError).ToArray();
        return TestRunParser.TestRunWithError(logLines);
    }

    private string FormatAsMsBuildError(Diagnostic diagnostic)
    {
        var message = diagnostic.GetMessage(System.Globalization.CultureInfo.CurrentUICulture);
        var projectSuffix = $" [{Path.GetFullPath(options.ProjectFilePath)}]";
        if (diagnostic.Location == Location.None || !diagnostic.Location.IsInSource)
            return $"CSC : error {diagnostic.Id}: {message}{projectSuffix}";

        var span = diagnostic.Location.GetMappedLineSpan();
        return $"{span.Path}({span.StartLinePosition.Line + 1},{span.StartLinePosition.Character + 1}): error {diagnostic.Id}: {message}{projectSuffix}";
    }

    /// <summary>
    /// Builds a folder in /tmp that looks like the project's output folder: the shape's dependencies
    /// (as symbolic links), and its deps.json and runtimeconfig.json renamed for this project.
    /// Linking avoids copying the dependencies on every run. We leave the folder in place, because
    /// each production run gets a fresh, empty /tmp.
    /// </summary>
    /// <returns>The path of the new folder.</returns>
    private string PrepareRunDirectory(string shapeDirectory)
    {
        var runDirectory = Path.Combine(Path.GetTempPath(), $"exercism-{Guid.NewGuid():N}");
        Directory.CreateDirectory(runDirectory);

        var shapeOutput = Path.Combine(shapeDirectory, OutputSubdirectory);
        foreach (var entry in Directory.EnumerateFileSystemEntries(shapeOutput))
        {
            var name = Path.GetFileName(entry);
            if (name.StartsWith(Token + ".", StringComparison.Ordinal))
            {
                if (name.EndsWith(".deps.json", StringComparison.Ordinal) || name.EndsWith(".runtimeconfig.json", StringComparison.Ordinal))
                    File.WriteAllText(Path.Combine(runDirectory, name.Replace(Token, ProjectName)), File.ReadAllText(entry).Replace(Token, ProjectName));
                continue;
            }

            File.CreateSymbolicLink(Path.Combine(runDirectory, name), entry);
        }

        return runDirectory;
    }

    private EmitResult Emit(CSharpCompilation compilation, string runDirectory)
    {
        using var peStream = File.Create(Path.Combine(runDirectory, $"{ProjectName}.dll"));
        using var pdbStream = File.Create(Path.Combine(runDirectory, $"{ProjectName}.pdb"));
        var emitOptions = new EmitOptions(
            debugInformationFormat: DebugInformationFormat.PortablePdb,
            pdbFilePath: Path.Combine(SolutionDirectory, IntermediateSubdirectory, $"{ProjectName}.pdb"));
        return compilation.Emit(peStream, pdbStream, options: emitOptions);
    }

    private TestRun RunTests(string runDirectory)
    {
        var reportPath = Path.Combine(runDirectory, "tests.xml");
        var assemblyPath = Path.Combine(runDirectory, $"{ProjectName}.dll");
        MSBuildTestrunner.RunProcess("dotnet", $"exec \"{assemblyPath}\" -xml \"{reportPath}\" -noLogo -noColor", runDirectory);
        return TestRunParser.TestRunWithoutError(reportPath, originalSyntaxTree);
    }

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
