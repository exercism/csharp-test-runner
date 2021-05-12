using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading;

using Microsoft.CodeAnalysis;

using Xunit.Runners;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunner
    {
        public static TestResult[] RunTests(Compilation compilation, Files files)
        {
            var outputPath = Path.Combine(Path.GetTempPath(), compilation.SourceModule.Name);
            compilation.Emit(outputPath);
            Assembly.LoadFrom(outputPath);

            var tests = new ConcurrentStack<TestInfo>();
            var finished = new ManualResetEventSlim();
            var runner = AssemblyRunner.WithoutAppDomain(outputPath);
            runner.OnTestFailed += info => tests.Push(info);
            runner.OnTestPassed += info => tests.Push(info);
            runner.OnExecutionComplete += _ => finished.Set();

            runner.Start();
            finished.Wait();

            return TestResultParser.FromTests(tests, compilation, files);
        }
    }
}