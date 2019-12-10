using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Exercism.TestRunner.CSharp
{
    internal class TestRun
    {
        public string Message { get; }
        public TestStatus Status { get; }
        public TestResult[] Tests { get; }

        private TestRun(string message, TestStatus status, TestResult[] tests) =>
            (Message, Status, Tests) = (message, status, tests);

        public static TestRun FromErrors(string errors) =>
            new TestRun(errors, TestStatus.Error, Array.Empty<TestResult>());
        
        public static TestRun FromTests(TestResult[] tests) =>
            new TestRun(null, ToTestStatus(tests), tests);

        private static TestStatus ToTestStatus(IReadOnlyCollection<TestResult> tests)
        {
            if (tests.Any(test => test.Status == TestStatus.Fail))
                return TestStatus.Fail;

            if (tests.All(test => test.Status == TestStatus.Pass))
                return TestStatus.Pass;

            return TestStatus.Error;
        }
    }
}