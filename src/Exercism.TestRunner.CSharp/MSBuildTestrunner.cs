using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp;

/// <summary>
/// Builds the solution with MSBuild and runs its tests. This works for every solution, and is the runner
/// used when <see cref="InMemoryCompiledTestRunner"/> can't handle the solution or is turned off.
/// </summary>
internal sealed class MSBuildTestrunner(Options options, SyntaxTree originalSyntaxTree, string originalProjectFile) : ITestRunner
{
    public const string AssemblyInfo = "[assembly: CaptureConsole]\n[assembly: CaptureTrace]\n";

    /// <summary>
    /// Builds the solution and runs its tests. It temporarily rewrites the solution's files for the build,
    /// and puts them back afterwards.
    /// </summary>
    /// <returns>The test run, which is never null.</returns>
    public TestRun Run()
    {
        BeforeTests();
        RunTests();
        AfterTests();

        return TestRunParser.Parse(options, originalSyntaxTree);
    }

    // We build the project, then run the xUnit v3 test executable directly. This is faster than
    // `dotnet test`, which would also start VSTest and a test host to run the same executable.
    private void RunTests()
    {
        var workingDirectory = Path.GetDirectoryName(options.TestsFilePath)!;
        RunProcess("dotnet", "restore --source /root/.nuget/packages/", workingDirectory);
        RunProcess("dotnet", "build -c release --no-restore --verbosity=quiet -nologo /flp:verbosity=quiet;errorsOnly=true", workingDirectory);

        if (File.Exists(options.AssemblyFilePath))
            RunProcess("dotnet", $"exec \"{options.AssemblyFilePath}\" -xml \"{options.TestResultsFilePath}\" -noLogo -noColor", Path.GetDirectoryName(options.AssemblyFilePath)!);
    }

    internal static void RunProcess(string command, string arguments, string workingDirectory)
    {
        var processStartInfo = new ProcessStartInfo(command, arguments)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        using var process = Process.Start(processStartInfo)!;
        // Read both streams, so that a process with a lot of output can't block on a full pipe.
        process.OutputDataReceived += (_, _) => { };
        process.ErrorDataReceived += (_, _) => { };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
    }

    private void BeforeTests()
    {
        RewriteProjectFile();
        RewriteTestsFile();
        AddCaptureOutputAssemblyAttributes();
    }

    /// <summary>
    /// Moves a project file onto the .NET version installed in the image.
    /// </summary>
    internal static string RewriteProjectFileText(string projectFileText) => projectFileText.Replace("net9.0", "net10.0");

    private void RewriteProjectFile() =>
        File.WriteAllText(options.ProjectFilePath, RewriteProjectFileText(originalProjectFile));

    private void RewriteTestsFile() =>
        File.WriteAllText(options.TestsFilePath, originalSyntaxTree.Rewrite().ToString());
        
    private void AddCaptureOutputAssemblyAttributes() =>
        File.WriteAllText(options.AssemblyInfoFilePath, AssemblyInfo);

    private void AfterTests()
    {
        UndoRewriteProjectFile();
        UndoRewriteTestsFile();
        UndoAddCaptureOutputAssemblyAttributes();
    }

    private void UndoRewriteProjectFile() => File.WriteAllText(options.ProjectFilePath, originalProjectFile);

    private void UndoRewriteTestsFile() => File.WriteAllText(options.TestsFilePath, originalSyntaxTree.ToString());

    private void UndoAddCaptureOutputAssemblyAttributes() => File.Delete(options.AssemblyInfoFilePath);
}
