using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace Exercism.TestRunner.CSharp;

/// <summary>
/// Compiles the solution with Roslyn inside this process and runs the xUnit v3 test executable directly,
/// skipping MSBuild and VSTest.
///
/// Why it is faster: `dotnet restore` and `dotnet test` start MSBuild, evaluate the project, resolve
/// packages, start the compiler server, then start VSTest and a test host. Most of that work gives the
/// same answer for every solution of an exercise. Here we do only the parts that depend on the solution:
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
/// test results go through the same code as the TRX results (<see cref="TestResultParser"/>). xUnit's XML
/// report escapes failure messages differently from VSTest, so <see cref="XunitXmlParser"/> converts them.
///
/// The fast path never writes into the solution folder. It returns null whenever it meets something it
/// does not fully understand, and the caller then runs the normal MSBuild path.
/// </summary>
internal sealed partial class FastPath(Options options, SyntaxTree originalSyntaxTree, string projectFileText, string? captureOutputAssemblyInfo)
{
    public const string ShapesDirectory = "/opt/test-runner/shapes";
    // The template project's name. We replace it with the solution's project name wherever it appears.
    private const string Token = "ExercismShape";
    // The template's ReadyToRun publish folder, which holds every dependency.
    private const string OutputSubdirectory = "publish";
    private const string IntermediateSubdirectory = "obj/release/net10.0";

    // Files that could change how MSBuild builds the project. If the solution folder has any of them,
    // the fast path will not run.
    private static readonly string[] DisallowedFileNames =
    [
        "directory.build.props", "directory.build.targets", "directory.packages.props", "directory.solution.props",
        "global.json", "nuget.config", "assemblyinfo.cs"
    ];

    private static readonly string[] DisallowedExtensions =
        [".props", ".targets", ".sln", ".slnx", ".resx", ".globalconfig", ".csproj", ".fsproj", ".vbproj", ".proj"];

    private string ProjectName => Path.GetFileNameWithoutExtension(options.ProjectFilePath);

    // MSBuild uses the project name when the project file sets no RootNamespace.
    private string RootNamespace => ProjectShape.RootNamespace(projectFileText) ?? ProjectName;

    private string SolutionDirectory => Path.GetDirectoryName(Path.GetFullPath(options.ProjectFilePath))!;

    // Returns the test run, or null if the caller should use the MSBuild path instead.
    // Every return of null goes through Fallback, which logs the reason to the console.
    public TestRun? TryRun()
    {
        var shapeDirectory = FindShapeDirectory();
        if (shapeDirectory is null)
            return Fallback("unknown project shape");

        var sourceFiles = CollectSourceFiles();
        if (sourceFiles is null)
            return Fallback("unusual files in the solution folder");

        var compilation = CreateCompilation(shapeDirectory, sourceFiles);
        if (compilation is null)
            return Fallback("could not prepare the compilation");

        var errors = CompileErrors(compilation);
        if (errors.Length > 0)
            return CompileErrorTestRun(errors);

        var runDirectory = PrepareRunDirectory(shapeDirectory);
        var emitResult = Emit(compilation, runDirectory);
        if (!emitResult.Success)
        {
            var emitErrors = emitResult.Diagnostics.Where(IsError).ToArray();
            return emitErrors.Length > 0 ? CompileErrorTestRun(emitErrors) : Fallback("emit failed without errors");
        }

        var testRun = RunTests(runDirectory);
        return testRun ?? Fallback("the test executable did not finish normally");
    }

    private static TestRun? Fallback(string reason)
    {
        Console.WriteLine($"Fast path not used: {reason}");
        return null;
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

    // Lists the C# files that MSBuild's default Compile glob would include (**/*.cs, apart from bin/, obj/
    // and folders that start with a dot), in the order it would find them. Returns null if the folder holds
    // anything that could make MSBuild compile differently.
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

    // An .editorconfig can change the severity of compiler diagnostics, which the fast path does not apply.
    private static bool EditorConfigChangesDiagnostics(string path)
    {
        var text = File.ReadAllText(path);
        return text.Contains("dotnet_diagnostic", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("dotnet_analyzer_diagnostic", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("is_global", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("build_property", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("generated_code", StringComparison.OrdinalIgnoreCase);
    }

    // Source generators and analyzers are not run by the fast path. Code that needs a generator, and
    // test attributes outside the test file (which the xUnit analyzers check), take the MSBuild path.
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

        if (captureOutputAssemblyInfo is not null)
            trees.Add(CSharpSyntaxTree.ParseText(SourceText.From(captureOutputAssemblyInfo, Encoding.UTF8), parseOptions, Path.Combine(SolutionDirectory, "AssemblyInfo.cs")));

        // The template's own two files come first on the command line. The rest are generated files,
        // which we move to where MSBuild would have put them for this project and rename.
        var templateFiles = new[] { $"{Token}.cs", $"{Token}Tests.cs" }.Select(name => Path.Combine(shapeDirectory, name)).ToHashSet();
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

    // csc stops after the first stage that has errors: parsing, then declarations. Method body errors
    // are found while emitting.
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
        return TestRunParser.FromBuildLog(logLines);
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

    // Builds a folder in /tmp that looks like the project's output folder: the shape's dependencies
    // (as symbolic links), and its deps.json and runtimeconfig.json renamed for this project.
    // Linking avoids copying the dependencies on every run. We leave the folder in place, because
    // each production run gets a fresh, empty /tmp.
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

    // Runs the test executable, which writes xUnit's XML report. Exit code 1 means some tests failed.
    // Any other non-zero exit code means the run itself went wrong, so we return null.
    private TestRun? RunTests(string runDirectory)
    {
        var assemblyPath = Path.Combine(runDirectory, $"{ProjectName}.dll");
        var reportPath = Path.Combine(runDirectory, "results.xml");

        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = runDirectory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var argument in new[] { "exec", assemblyPath, "-xml", reportPath, "-noLogo", "-noColor" })
            startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo)!;
        // Drain both streams, so that a chatty test can never block on a full pipe.
        process.OutputDataReceived += (_, _) => { };
        process.ErrorDataReceived += (_, _) => { };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode is not (0 or 1))
            return null;

        var rawResults = XunitXmlParser.Parse(reportPath);
        if (rawResults is null)
            return null;

        Console.WriteLine("Fast path used: tests run");
        return TestRunParser.FromRawResults(rawResults, originalSyntaxTree);
    }

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
