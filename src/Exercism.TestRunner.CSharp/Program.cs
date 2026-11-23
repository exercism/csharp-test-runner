using CommandLine;

using Microsoft.CodeAnalysis.CSharp;

namespace Exercism.TestRunner.CSharp;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args is ["--prepare-shape", var shapeDirectory])
            return InMemoryCompiledTestRunner.PrepareShape(shapeDirectory);

        Parser.Default
            .ParseArguments<Options>(args)
            .WithParsed(CreateTestResults);
        return 0;
    }

    private static void CreateTestResults(Options options)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:u}] Running test runner for '{options.Slug}' solution...");

        var originalSyntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(options.TestsFilePath));
        var originalProjectFile = File.ReadAllText(options.ProjectFilePath);
        var inMemoryCompiledTestRunner = new InMemoryCompiledTestRunner(options, originalSyntaxTree, originalProjectFile);
        var msBuildTestRunner = new MSBuildTestrunner(options, originalSyntaxTree, originalProjectFile);

        // We try the in-memory runner first, because it is much faster. It returns null if it can't handle
        // the solution, and then MSBuild runs the tests.
        var testRun = (InMemoryCompiledTestRunnerEnabled ? inMemoryCompiledTestRunner.Run() : null) ?? msBuildTestRunner.Run();
        testRun.WriteToFile(options.ResultsJsonFilePath);

        Console.WriteLine($"[{DateTimeOffset.UtcNow:u}] Ran test runner for '{options.Slug}' solution");
    }

    /// <summary>
    /// Setting EXERCISM_FAST_PATH=off turns the in-memory runner off, to compare its results with MSBuild's.
    /// </summary>
    private static bool InMemoryCompiledTestRunnerEnabled =>
        Environment.GetEnvironmentVariable("EXERCISM_FAST_PATH") != "off";
}