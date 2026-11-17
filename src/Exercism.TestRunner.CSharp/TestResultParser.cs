using System.Xml.Serialization;

using Humanizer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Win32.SafeHandles;

namespace Exercism.TestRunner.CSharp;

internal static class TestResultParser
{
    internal static TestResult[] FromFile(string logFilePath, SyntaxTree testsSyntaxTree)
    {
        using var fileStream = File.OpenRead(logFilePath);
        var result = new XmlSerializer(typeof(XmlTestRun)).Deserialize(fileStream) as XmlTestRun;

        if (result?.Results?.UnitTestResult is null)
            return [];

        var rawResults = result.Results.UnitTestResult.Select(unitTestResult =>
            new RawTestResult(unitTestResult.TestName, unitTestResult.Outcome, unitTestResult.Output?.StdOut, unitTestResult.Output?.ErrorInfo?.Message));
        return ToTestResults(rawResults, testsSyntaxTree);
    }

    internal static TestResult[] ToTestResults(IEnumerable<RawTestResult> rawResults, SyntaxTree testsSyntaxTree)
    {
        var methodDeclarations = 
            testsSyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .ToArray();
            
        var testResults =
            from unitTestResult in rawResults
            let testMethodDeclaration = unitTestResult.TestMethod(methodDeclarations)
            orderby testMethodDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line
            select ToTestResult(unitTestResult, testMethodDeclaration);

        return testResults.ToArray();
    }

    private static TestResult ToTestResult(RawTestResult rawResult, MethodDeclarationSyntax testMethodDeclaration) =>
        new()
        {
            Name = rawResult.Name(),
            Status = rawResult.Status(),
            Message = rawResult.Message(),
            Output = rawResult.Output(),
            TaskId = testMethodDeclaration.TaskId(),
            TestCode = testMethodDeclaration.TestCode()
        };

    private static MethodDeclarationSyntax TestMethod(this RawTestResult rawResult, IEnumerable<MethodDeclarationSyntax> methodDeclarations)
    {
        var classAndMethodName = rawResult.TestName.Split(".");
        var className = classAndMethodName[0];
        var methodName = classAndMethodName[1].Split('(')[0];

        return methodDeclarations.Single(method =>
            method.Identifier.Text == methodName &&
            method.Parent is ClassDeclarationSyntax classDeclaration &&
            classDeclaration.Identifier.Text == className);
    }

    private static string Name(this RawTestResult rawResult) =>
        rawResult.TestName
            .Substring(rawResult.TestName.LastIndexOf(".", StringComparison.Ordinal) + 1)
            .Humanize();

    private static TestStatus Status(this RawTestResult rawResult) =>
        rawResult.Outcome switch
        {
            "Passed" => TestStatus.Pass,
            "Failed" => TestStatus.Fail,
            _ => TestStatus.Error
        };

    private static string? Message(this RawTestResult rawResult) =>
        rawResult.FailureMessage?.UseUnixNewlines()?.Trim();

    private static string? Output(this RawTestResult rawResult) =>
        rawResult.StdOut?.UseUnixNewlines()?.Trim();

    private static string TestCode(this MethodDeclarationSyntax testMethod)
    {
        if (testMethod.Body is not null)
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
            .Select(attribute => (LiteralExpressionSyntax)attribute.ArgumentList!.Arguments[0].Expression)
            .Select(taskNumberExpression => (int?)taskNumberExpression.Token.Value!)
            .FirstOrDefault();
}

/// <summary>One test's result as read from a TRX file or an xUnit XML report.</summary>
internal sealed record RawTestResult(string TestName, string Outcome, string? StdOut, string? FailureMessage);

[XmlRoot(ElementName = "Output", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
public sealed class XmlOutput
{
    [XmlElement(ElementName = "StdOut", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public string? StdOut { get; set; }

    [XmlElement(ElementName = "ErrorInfo",
        Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public XmlErrorInfo? ErrorInfo { get; set; }
}

[XmlRoot(ElementName = "UnitTestResult", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
public sealed class XmlUnitTestResult
{
    [XmlElement(ElementName = "Output", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public XmlOutput? Output { get; set; }

    [XmlAttribute(AttributeName = "testName")]
    public required string TestName { get; set; }

    [XmlAttribute(AttributeName = "outcome")]
    public required string Outcome { get; set; }
}

[XmlRoot(ElementName = "ErrorInfo", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
public sealed class XmlErrorInfo
{
    [XmlElement(ElementName = "Message", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public string? Message { get; set; }
}

[XmlRoot(ElementName = "Results", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
public sealed class XmlResults
{
    [XmlElement(ElementName = "UnitTestResult",
        Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public List<XmlUnitTestResult>? UnitTestResult { get; set; }
}

[XmlRoot(ElementName = "TestRun", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
public sealed class XmlTestRun
{
    [XmlElement(ElementName = "Results", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public required XmlResults Results { get; set; }
}