using System.Text.RegularExpressions;
using System.Xml.Linq;

using Humanizer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Exercism.TestRunner.CSharp;

internal static partial class TestResultParser
{
    internal static TestResult[] FromFile(string logFilePath, SyntaxTree testsSyntaxTree) =>
        XDocument.Load(logFilePath)
            .Descendants("test")
            .ToTestResults(testsSyntaxTree);

    private static TestResult[] ToTestResults(this IEnumerable<XElement> tests, SyntaxTree testsSyntaxTree)
    {
        var methodDeclarations = 
            testsSyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .ToArray();
            
        var testResults =
            from test in tests
            let testMethodDeclaration = test.TestMethod(methodDeclarations)
            orderby testMethodDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line
            select ToTestResult(test, testMethodDeclaration);

        return testResults.ToArray();
    }

    private static TestResult ToTestResult(XElement test, MethodDeclarationSyntax testMethodDeclaration) =>
        new()
        {
            Name = test.Name(),
            Status = test.Status(),
            Message = test.Message(),
            Output = test.Output(),
            TaskId = testMethodDeclaration.TaskId(),
            TestCode = testMethodDeclaration.TestCode()
        };

    private static MethodDeclarationSyntax TestMethod(this XElement test, IEnumerable<MethodDeclarationSyntax> methodDeclarations)
    {
        var classAndMethodName = test.TestName().Split(".");
        var className = classAndMethodName[0];
        var methodName = classAndMethodName[1].Split('(')[0];

        return methodDeclarations.Single(method =>
            method.Identifier.Text == methodName &&
            method.Parent is ClassDeclarationSyntax classDeclaration &&
            classDeclaration.Identifier.Text == className);
    }

    private static string Name(this XElement test) =>
        test.TestName()
            .Substring(test.TestName().LastIndexOf(".", StringComparison.Ordinal) + 1)
            .Humanize();

    private static TestStatus Status(this XElement test) =>
        test.Attribute("result")?.Value switch
        {
            "Pass" => TestStatus.Pass,
            "Fail" => TestStatus.Fail,
            _ => TestStatus.Error
        };

    private static string? Message(this XElement test) =>
        test.Element("failure")?.Element("message")?.Value.Unescape().UseUnixNewlines().Trim();

    private static string? Output(this XElement test) =>
        test.Element("output")?.Value.UseUnixNewlines().Trim();

    private static string TestName(this XElement test) => test.Attribute("name")!.Value;

    // xUnit's XML report escapes some characters in failure messages with a backslash, such as \n for a
    // newline and \x01 for other control characters. This turns them back into the original characters.
    private static string Unescape(this string message) =>
        EscapedCharacter().Replace(message, match => match.Groups[1].Value switch
        {
            "n" => "\n",
            "r" => "\r",
            "t" => "\t",
            "0" => "\0",
            "a" => "\a",
            "b" => "\b",
            "f" => "\f",
            "v" => "\v",
            "\\" or "\"" => match.Groups[1].Value,
            { Length: 3 } hex => ((char)Convert.ToInt32(hex[1..], 16)).ToString(),
            _ => match.Value
        });

    [GeneratedRegex(@"\\(x[01][0-9a-f]|.)")]
    private static partial Regex EscapedCharacter();

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
