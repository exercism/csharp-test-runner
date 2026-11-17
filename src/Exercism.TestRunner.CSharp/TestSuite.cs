using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Exercism.TestRunner.CSharp;

internal sealed class TestSuite(SyntaxTree originalSyntaxTree, string originalProjectFile, Options options)
{
    private const string AssemblyInfo = "[assembly: CaptureConsole]\n[assembly: CaptureTrace]\n";

    private const string BuildLogArgument = "/flp:verbosity=quiet;errorsOnly=true";

    // An xUnit v3 project can take up to three routes, from fastest to slowest. Each one hands over to
    // the next as soon as anything looks unusual, so all three give the same results.
    //   1. The fast path (see FastPath) compiles with Roslyn in this process, using compiler arguments
    //      captured at image build, and runs the test executable. It never runs MSBuild or VSTest.
    //   2. If the fast path gives up, MSBuild restores and builds the project as before, and we run the
    //      test executable directly, which still skips VSTest (see BuildAndRunTestExecutable).
    //   3. If that gives no usable report, we run `dotnet test` exactly as before.
    // Other projects (such as xUnit v2 ones) always go straight to route 3.
    // Setting EXERCISM_FAST_PATH=off skips route 1, which is useful for comparing the routes' results.
    public TestRun Run()
    {
        if (Environment.GetEnvironmentVariable("EXERCISM_FAST_PATH") != "off" && RunTestExecutableDirectly)
        {
            var assemblyInfo = CaptureOutput ? AssemblyInfo : null;
            var fastTestRun = new FastPath(options, originalSyntaxTree, RewrittenProjectFile, assemblyInfo).TryRun();
            if (fastTestRun is not null)
                return fastTestRun;
        }

        BeforeTests();
        var testRun = RunTests();
        AfterTests();

        return testRun;
    }

    private TestRun RunTests()
    {
        var workingDirectory = Path.GetDirectoryName(options.TestsFilePath)!;
        RunProcess("dotnet", "restore --source /root/.nuget/packages/", workingDirectory);

        if (RunTestExecutableDirectly)
        {
            var testRun = BuildAndRunTestExecutable(workingDirectory);
            if (testRun is not null)
                return testRun;
        }

        RunProcess("dotnet", $"test -c release --no-restore --verbosity=quiet --logger \"trx;LogFileName={Path.GetFileName(options.TestResultsFilePath)}\" {BuildLogArgument}", workingDirectory);
        return TestRunParser.Parse(options, originalSyntaxTree);
    }

    // Builds the project with MSBuild, then runs the xUnit v3 test executable itself. This skips VSTest
    // and its test host, which `dotnet test` would start. The executable writes xUnit's XML report, and
    // XunitXmlParser turns it into the same results as the TRX file.
    // Returns null when the tests did not finish normally or the report holds anything unusual, so that
    // the caller can run them through `dotnet test` instead.
    private TestRun? BuildAndRunTestExecutable(string workingDirectory)
    {
        RunProcess("dotnet", $"build -c release --no-restore --verbosity=quiet -nologo {BuildLogArgument}", workingDirectory);

        if (TestRunParser.BuildFailed(options))
            return TestRunParser.Parse(options, originalSyntaxTree);

        var assemblyName = Path.GetFileNameWithoutExtension(options.ProjectFilePath);
        var outputDirectory = Path.Combine(workingDirectory, "bin", "release", "net10.0");
        var assemblyPath = Path.Combine(outputDirectory, $"{assemblyName}.dll");
        var reportPath = Path.Combine(Path.GetTempPath(), $"exercism-{Guid.NewGuid():N}.xml");

        var exitCode = RunProcess("dotnet", $"exec \"{assemblyPath}\" -xml \"{reportPath}\" -noLogo -noColor", outputDirectory);
        if (exitCode is not (0 or 1))
            return null;

        var rawResults = XunitXmlParser.Parse(reportPath);
        File.Delete(reportPath);
        if (rawResults is null)
            return null;

        return TestRunParser.FromRawResults(rawResults, originalSyntaxTree);
    }

    private static int RunProcess(string command, string arguments, string workingDirectory)
    {
        var processStartInfo = new ProcessStartInfo(command, arguments)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        using var process = Process.Start(processStartInfo)!;
        // Drain both streams, so that a chatty process can never block on a full pipe.
        process.OutputDataReceived += (_, _) => { };
        process.ErrorDataReceived += (_, _) => { };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        return process.ExitCode;
    }

    private void BeforeTests()
    {
        RewriteProjectFile();
        RewriteTestsFile();
            
        if (CaptureOutput)
            AddCaptureOutputAssemblyAttributes();
    }

    private string RewrittenProjectFile => originalProjectFile.Replace("net9.0", "net10.0");

    private void RewriteProjectFile() =>
        File.WriteAllText(options.ProjectFilePath, RewrittenProjectFile);

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

    // An xUnit v3 test project builds to an executable that can run its own tests.
    private bool RunTestExecutableDirectly =>
        CaptureOutput && originalProjectFile.Contains("<OutputType>Exe</OutputType>");

    public static TestSuite FromOptions(Options options)
    {
        var originalSyntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(options.TestsFilePath));
        var originalProjectFile = File.ReadAllText(options.ProjectFilePath);
        return new TestSuite(originalSyntaxTree, originalProjectFile, options);
    }
}