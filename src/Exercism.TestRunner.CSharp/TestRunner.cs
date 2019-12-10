using System.Threading.Tasks;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestRunner
    {
        public static async Task<TestRun> Run(Options options)
        {
            var compilation = await ProjectCompiler.Compile(options);
            if (compilation.HasErrors())
                return TestRun.FromErrors(TestRunMessage.FromErrors(compilation.GetErrors()));

            return await CompilationTestRunner.Run(compilation);
        }
    }
}