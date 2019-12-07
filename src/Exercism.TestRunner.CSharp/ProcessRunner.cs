using System;
using System.Diagnostics;

namespace Exercism.TestRunner.CSharp
{
    internal static class ProcessRunner
    {
        public static (string output, int exitCode) Run(string fileName, string arguments)
        {
            var processStartInfo = new ProcessStartInfo(fileName, arguments) { RedirectStandardOutput = true };
            var process = Process.Start(processStartInfo);
            if (process == null)
                throw new InvalidOperationException($"Could not run {fileName} {arguments}");
            
            process.WaitForExit();

            return (output: process.StandardOutput.ReadToEnd(), exitCode: process.ExitCode);
        }
    }
}