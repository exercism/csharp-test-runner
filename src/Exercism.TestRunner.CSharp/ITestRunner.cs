namespace Exercism.TestRunner.CSharp;

/// <summary>
/// Builds a solution and runs its tests.
/// </summary>
internal interface ITestRunner
{
    /// <summary>
    /// Builds the solution and runs its tests.
    /// </summary>
    /// <returns>The test run, or null if this runner can't handle the solution and another runner should run it.</returns>
    TestRun? Run();
}
