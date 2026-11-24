using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp;

/// <summary>
/// Builds the solution with MSBuild and runs its tests. This works for every solution, and is the runner
/// used when <see cref="InMemoryCompiledTestRunner"/> can't handle the solution or is turned off.
/// </summary>
internal sealed class MSBuildTestrunner(Options options, SyntaxTree originalSyntaxTree, string originalProjectFile)
{
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
        ProcessRunner.Run("dotnet", "restore --source /root/.nuget/packages/", workingDirectory);
        ProcessRunner.Run("dotnet", "build -c release --no-restore --verbosity=quiet -nologo /flp:verbosity=quiet;errorsOnly=true", workingDirectory);

        if (File.Exists(options.AssemblyFilePath)) ProcessRunner.Run("dotnet", $"exec \"{options.AssemblyFilePath}\" -xml \"{options.TestResultsFilePath}\" -noLogo -noColor", Path.GetDirectoryName(options.AssemblyFilePath)!);
    }

    private void BeforeTests()
    {
        RewriteProjectFile();
        RewriteTestsFile();
        AddCaptureOutputAssemblyAttributes();
    }

    private void RewriteProjectFile() =>
        File.WriteAllText(options.ProjectFilePath, Rewriter.RewriteProjectFileText(originalProjectFile));

    private void RewriteTestsFile() =>
        File.WriteAllText(options.TestsFilePath, originalSyntaxTree.RewriteTests().ToString());
        
    private void AddCaptureOutputAssemblyAttributes() =>
        File.WriteAllText(options.AssemblyInfoFilePath, Rewriter.AssemblyInfo);

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
