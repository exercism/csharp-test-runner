using CommandLine;

namespace Exercism.TestRunner.CSharp;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args is ["--prepare-shape", var shapeDirectory])
            return FastPath.PrepareShape(shapeDirectory);

        Parser.Default
            .ParseArguments<Options>(args)
            .WithParsed(CreateTestResults);
        return 0;
    }

    private static void CreateTestResults(Options options)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:u}] Running test runner for '{options.Slug}' solution...");

        var testSuite = TestSuite.FromOptions(options);
        var testRun = testSuite.Run();
        testRun.WriteToFile(options.ResultsJsonFilePath);

        Console.WriteLine($"[{DateTimeOffset.UtcNow:u}] Ran test runner for '{options.Slug}' solution");
    }
}