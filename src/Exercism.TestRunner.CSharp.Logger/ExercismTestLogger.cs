using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Microsoft.VisualStudio.TestPlatform.Extension.Exercism.TestLogger
{
    /// <summary>
    /// This logger can be called using:
    /// 
    /// dotnet test --logger:exercism;ResultsDirectoryName=[results-directory] --test-adapter-path:[adapter-directory]
    /// </summary>
    [FriendlyName(FriendlyName)]
    [ExtensionUri(ExtensionUri)]
    public class ExercismTestLogger : ITestLoggerWithParameters
    {
        private const string ExtensionUri = "logger://Microsoft/TestPlatform/ExercismTestLogger/v1";
        private const string FriendlyName = "exercism";

        private Options _options;
        private List<TestResultEventArgs> _testResultEvents;

        public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (parameters.Count == 0)
                throw new ArgumentException("No default parameters added", nameof(parameters));
            
            var testRunDirectory = GetTestResultsDirectory(parameters);
            Initialize(events, testRunDirectory);
        }

        private static string GetTestResultsDirectory(IReadOnlyDictionary<string, string> parameters) =>
            parameters.GetValueOrDefault("ResultsDirectoryName", parameters[DefaultLoggerParameterNames.TestRunDirectory]);

        public void Initialize(TestLoggerEvents events, string testRunDirectory)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            if (string.IsNullOrEmpty(testRunDirectory))
                throw new ArgumentNullException(nameof(testRunDirectory));

            _options = new Options(testRunDirectory);
            _testResultEvents = new List<TestResultEventArgs>();
            
            events.TestResult += TestResultHandler;
            events.TestRunComplete += TestRunCompleteHandler;
        }

        private void TestResultHandler(object sender, TestResultEventArgs e) =>
            _testResultEvents.Add(e);

        private void TestRunCompleteHandler(object sender, TestRunCompleteEventArgs e)
        {
            var testRun = TestResultEventArgsConverter.ToTestRun(_testResultEvents);
            TestRunWriter.WriteToFile(_options, testRun);
        }
    }
}