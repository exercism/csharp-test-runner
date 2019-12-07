using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Microsoft.VisualStudio.TestPlatform.Extension.Exercism.TestLogger
{
    internal static class TestResultEventArgsConverter
    {
        public static TestRun ToTestRun(IReadOnlyCollection<TestResultEventArgs> testResultEvents)
        {
            var testResults = ToTestResults(testResultEvents);
            var testStatus = ToTestStatus(testResultEvents);
            
            return new TestRun(message: null, testStatus, testResults);
        }

        private static TestResult[] ToTestResults(IEnumerable<TestResultEventArgs> testResultEvents) =>
            testResultEvents.Select(ToTestResult).ToArray();

        private static TestResult ToTestResult(TestResultEventArgs testResultEvent) =>
            new TestResult(testResultEvent.Result.DisplayName, testResultEvent.Result.ErrorMessage, ToTestStatus(testResultEvent.Result.Outcome));

        private static TestStatus ToTestStatus(TestOutcome testOutcome) =>
            testOutcome switch
            {
                TestOutcome.Passed => TestStatus.Pass,
                TestOutcome.Failed => TestStatus.Fail,
                _ => TestStatus.Error
            };

        private static TestStatus ToTestStatus(IReadOnlyCollection<TestResultEventArgs> testResultEvents)
        {
            if (testResultEvents.Any(testResultEvent => testResultEvent.Result.Outcome == TestOutcome.Failed))
                return TestStatus.Fail;
            
            if (testResultEvents.All(testResultEvent => testResultEvent.Result.Outcome == TestOutcome.Passed))
                return TestStatus.Pass;

            return TestStatus.Error;
        }
    }
}