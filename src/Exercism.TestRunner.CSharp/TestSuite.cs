using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Exercism.TestRunner.CSharp;

internal sealed class TestSuite(SyntaxTree originalSyntaxTree, string originalProjectFile, Options options)
{
    private const string AssemblyInfo = "[assembly: CaptureConsole]\n[assembly: CaptureTrace]\n";

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

    private static void RunProcess(string command, string arguments, string workingDirectory)
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

    private void RewriteProjectFile() =>
        File.WriteAllText(options.ProjectFilePath,
            originalProjectFile.Replace("net9.0", "net10.0"));

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

    public static TestSuite FromOptions(Options options)
    {
        var originalSyntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(options.TestsFilePath));
        var originalProjectFile = File.ReadAllText(options.ProjectFilePath);
        return new TestSuite(originalSyntaxTree, originalProjectFile, options);
    }
}
