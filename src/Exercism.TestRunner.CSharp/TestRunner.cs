using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunner
    {
        public static async Task<TestRun> RunTests(Options options)
        {
            var testProject = await TestProjectReader.FromOptions(options);
            var testProjectRewriter = new TestProjectRewriter(testProject);
            
            try
            {
                await testProjectRewriter.Rewrite();
                RunDotnetTest(options);
                return TestRunParser.Parse(options);
            }
            finally
            {
                testProjectRewriter.UndoRewrite();
            }
        }

        private static void RunDotnetTest(Options options)
        {
            var command = "dotnet";
            var arguments = $"test --verbosity=quiet --logger \"trx;LogFileName={Path.GetFileName(options.TestResultsFilePath)}\" /flp:v=q";

            var processStartInfo = new ProcessStartInfo(command, arguments);
            processStartInfo.WorkingDirectory = Path.GetDirectoryName(options.ProjectFilePath)!;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            Process.Start(processStartInfo)?.WaitForExit();
        }
    }
}