using System.Diagnostics;

namespace Exercism.TestRunner.CSharp;

internal static class ProcessRunner
{
    internal static void Run(string command, string arguments, string workingDirectory)
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
}
