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

    private void RunTests()
    {
        var workingDirectory = Path.GetDirectoryName(options.TestsFilePath)!;
        RunProcess("dotnet", "restore --source /root/.nuget/packages/", workingDirectory);
        RunProcess("dotnet", $"test -c release --no-restore --verbosity=quiet --logger \"trx;LogFileName={Path.GetFileName(options.TestResultsFilePath)}\" /flp:verbosity=quiet;errorsOnly=true", workingDirectory);            
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
        Process.Start(processStartInfo)?.WaitForExit();
    }

    private void BeforeTests()
    {
        RewriteProjectFile();
        RewriteTestsFile();
            
        if (CaptureOutput)
            AddCaptureOutputAssemblyAttributes();
    }

    private void RewriteProjectFile() =>
        File.WriteAllText(options.ProjectFilePath,
            originalProjectFile
                .Replace("net5.0", "net9.0")
                .Replace("net6.0", "net9.0")
                .Replace("net7.0", "net9.0")
                .Replace("net8.0", "net9.0"));

    private void RewriteTestsFile() =>
        File.WriteAllText(options.TestsFilePath, originalSyntaxTree.Rewrite().ToString());
        
    private void AddCaptureOutputAssemblyAttributes() =>
        File.WriteAllText(options.AssemblyInfoFilePath, AssemblyInfo);

    private void AfterTests()
    {
        UndoRewriteProjectFile();
        UndoRewriteTestsFile();
            
        if (CaptureOutput)
            UndoAddCaptureOutputAssemblyAttributes();
    }

    private void UndoRewriteProjectFile() => File.WriteAllText(options.ProjectFilePath, originalProjectFile);

    private void UndoRewriteTestsFile() => File.WriteAllText(options.TestsFilePath, originalSyntaxTree.ToString());

    private void UndoAddCaptureOutputAssemblyAttributes() => File.Delete(options.AssemblyInfoFilePath);

    private bool CaptureOutput => originalProjectFile.Contains("xunit.v3");

    public static TestSuite FromOptions(Options options)
    {
        var originalSyntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(options.TestsFilePath));
        var originalProjectFile = File.ReadAllText(options.ProjectFilePath);
        return new TestSuite(originalSyntaxTree, originalProjectFile, options);
    }
}