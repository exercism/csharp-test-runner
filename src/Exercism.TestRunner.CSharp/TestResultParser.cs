using System.Xml.Serialization;

using Humanizer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestResultParser
    {
        internal static TestResult[] FromFile(string logFilePath, SyntaxTree testsSyntaxTree)
        {
            using var fileStream = File.OpenRead(logFilePath);
            var result = (XmlTestRun)new XmlSerializer(typeof(XmlTestRun)).Deserialize(fileStream);

            if (result.Results == null)
                return Array.Empty<TestResult>();

            return result.ToTestResults(testsSyntaxTree);
        }

        private static TestResult[] ToTestResults(this XmlTestRun result, SyntaxTree testsSyntaxTree)
        {
            var methodDeclarations = 
                testsSyntaxTree
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .ToArray();
            
            var testResults =
                from unitTestResult in result.Results.UnitTestResult
                let testMethodDeclaration = unitTestResult.TestMethod(methodDeclarations)
                orderby testMethodDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line
                select ToTestResult(unitTestResult, testMethodDeclaration);

            return testResults.ToArray();
        }

        private static TestResult ToTestResult(XmlUnitTestResult xmlUnitTestResult, MethodDeclarationSyntax testMethodDeclaration) =>
            new()
            {
                Name = xmlUnitTestResult.Name(),
                Status = xmlUnitTestResult.Status(),
                Message = xmlUnitTestResult.Message(),
                Output = xmlUnitTestResult.Output(),
                TaskId = testMethodDeclaration.TaskId(),
                TestCode = testMethodDeclaration.TestCode()
            };

        private static MethodDeclarationSyntax TestMethod(this XmlUnitTestResult xmlUnitTestResult, IEnumerable<MethodDeclarationSyntax> methodDeclarations)
        {
            var classAndMethodName = xmlUnitTestResult.TestName.Split(".");
            var className = classAndMethodName[0];
            var methodName = classAndMethodName[1];

            return methodDeclarations.Single(method =>
                method.Identifier.Text == methodName &&
                method.Parent is ClassDeclarationSyntax classDeclaration &&
                classDeclaration.Identifier.Text == className);
        }

        private static string Name(this XmlUnitTestResult xmlUnitTestResult) =>
            xmlUnitTestResult.TestName
                .Substring(xmlUnitTestResult.TestName.LastIndexOf(".", StringComparison.Ordinal) + 1)
                .Humanize();

        private static TestStatus Status(this XmlUnitTestResult xmlUnitTestResult) =>
            xmlUnitTestResult.Outcome switch
            {
                "Passed" => TestStatus.Pass,
                "Failed" => TestStatus.Fail,
                _ => TestStatus.Error
            };

        private static string Message(this XmlUnitTestResult xmlUnitTestResult) =>
            xmlUnitTestResult.Output?.ErrorInfo?.Message?.UseUnixNewlines()?.Trim();

        private static string Output(this XmlUnitTestResult xmlUnitTestResult) =>
            xmlUnitTestResult.Output?.StdOut?.UseUnixNewlines()?.Trim();

        private static string TestCode(this MethodDeclarationSyntax testMethod)
        {
            if (testMethod.Body != null)
                return SyntaxFactory.List(testMethod.Body.Statements.Select(statement => statement.WithoutLeadingTrivia())).ToString();

            return testMethod.ExpressionBody!
                .Expression
                .WithoutLeadingTrivia()
                .ToString();
        }

        private static int? TaskId(this MethodDeclarationSyntax testMethod) =>
            testMethod.AttributeLists
                .SelectMany(attributeList => attributeList.Attributes)
                .Where(attribute =>
                    attribute.Name.ToString() == "Task" &&
                    attribute.ArgumentList != null &&
                    attribute.ArgumentList.Arguments.Count == 1 &&
                    attribute.ArgumentList.Arguments[0].Expression.IsKind(SyntaxKind.NumericLiteralExpression))
                .Select(attribute => (LiteralExpressionSyntax)attribute.ArgumentList.Arguments[0].Expression)
                .Select(taskNumberExpression => (int?)taskNumberExpression.Token.Value!)
                .FirstOrDefault();
    }

    [XmlRoot(ElementName = "Output", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public class XmlOutput
    {
        [XmlElement(ElementName = "StdOut", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
        public string StdOut { get; set; }

        [XmlElement(ElementName = "ErrorInfo",
            Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
        public XmlErrorInfo ErrorInfo { get; set; }
    }

    [XmlRoot(ElementName = "UnitTestResult", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public class XmlUnitTestResult
    {
        [XmlElement(ElementName = "Output", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
        public XmlOutput Output { get; set; }

        [XmlAttribute(AttributeName = "testName")]
        public string TestName { get; set; }

        [XmlAttribute(AttributeName = "outcome")]
        public string Outcome { get; set; }
    }

    [XmlRoot(ElementName = "ErrorInfo", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public class XmlErrorInfo
    {
        [XmlElement(ElementName = "Message", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
        public string Message { get; set; }
    }

    [XmlRoot(ElementName = "Results", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public class XmlResults
    {
        [XmlElement(ElementName = "UnitTestResult",
            Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
        public List<XmlUnitTestResult> UnitTestResult { get; set; }
    }

    [XmlRoot(ElementName = "TestRun", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public class XmlTestRun
    {
        [XmlElement(ElementName = "Results", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
        public XmlResults Results { get; set; }
    }
}