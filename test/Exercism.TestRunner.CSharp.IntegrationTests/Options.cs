using System;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    public static class Options
    {
        public static bool UseDocker =>
            GetBooleanEnvironmentVariable("USE_DOCKER");

        private static bool GetBooleanEnvironmentVariable(string name) =>
            bool.TryParse(Environment.GetEnvironmentVariable(name), out var enabled) && enabled;
    }
}