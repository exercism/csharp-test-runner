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

        // Try the in-memory runner first and fallback to the MSBuild one if needed
        var testRun = new InMemoryCompiledTestRunner(options, originalSyntaxTree, originalProjectFile).Run() ??
                      new MSBuildTestrunner(options, originalSyntaxTree, originalProjectFile).Run();
        testRun.WriteToFile(options.ResultsJsonFilePath);

        Console.WriteLine($"[{DateTimeOffset.UtcNow:u}] Ran test runner for '{options.Slug}' solution");
    }
}
