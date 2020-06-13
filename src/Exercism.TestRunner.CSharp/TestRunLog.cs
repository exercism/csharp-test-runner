using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Exercism.TestRunner.CSharp
{
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    internal static class TestRunLog
    {
        internal static TestResult[] Parse(string projectDirectory)
        {
            var xmlSerializer = new XmlSerializer(typeof(XmlTestRun));
            using var fileStream = File.OpenRead("/Users/erik/Code/exercism/csharp-test-runner/test/Exercism.TestRunner.CSharp.IntegrationTests/Solutions/MultipleTestsWithTestOutput/TestResults/tests.trx");
            var result = xmlSerializer.Deserialize(fileStream);

            return Array.Empty<TestResult>();
        }
        
        [XmlRoot(ElementName = "Output", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
        private class XmlOutput
        {
            [XmlElement(ElementName = "StdOut", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
            public string StdOut { get; set; }

            [XmlElement(ElementName = "ErrorInfo",
                Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
            public XmlErrorInfo ErrorInfo { get; set; }
        }

        [XmlRoot(ElementName = "UnitTestResult", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
        private class XmlUnitTestResult
        {
            [XmlElement(ElementName = "Output", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
            public XmlOutput Output { get; set; }

            [XmlAttribute(AttributeName = "testName")]
            public string TestName { get; set; }

            [XmlAttribute(AttributeName = "computerName")]
            public string ComputerName { get; set; }

            [XmlAttribute(AttributeName = "outcome")]
            public string Outcome { get; set; }
        }

        [XmlRoot(ElementName = "ErrorInfo", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
        private class XmlErrorInfo
        {
            [XmlElement(ElementName = "Message", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
            public string Message { get; set; }

            [XmlElement(ElementName = "StackTrace",
                Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
            public string StackTrace { get; set; }
        }

        [XmlRoot(ElementName = "Results", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
        private class XmlResults
        {
            [XmlElement(ElementName = "UnitTestResult",
                Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
            public List<XmlUnitTestResult> UnitTestResult { get; set; }
        }

        [XmlRoot(ElementName = "TestRun", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
        private class XmlTestRun
        {
            [XmlElement(ElementName = "Results", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
            public XmlResults Results { get; set; }

            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }
}