using System.Linq;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Abstractions;

namespace Exercism.TestRunner.CSharp
{
    internal class TestResult
    {
        public string Name { get; }
        public string Test { get; }
        public string Expected { get; }
        public string Message { get; }
        public string Output { get; }
        public TestStatus Status { get; }

        private TestResult(string name, string test, string expected, TestStatus status, string message, string output) =>
            (Name, Test, Expected, Message, Status, Output) = (name, test, expected, message, status, output);

        public static TestResult FromPassed(ITestPassed test, SyntaxNode syntaxNode) =>
            new TestResult(ToName(test.TestCase), ToTest(test.TestCase), ToExpected(syntaxNode), TestStatus.Pass, null, test.Output);

        public static TestResult FromFailed(ITestFailed test, SyntaxNode syntaxNode) =>
            new TestResult(ToName(test.TestCase), ToTest(test.TestCase), ToExpected(syntaxNode), TestStatus.Fail, TestRunMessage.FromMessages(test.Messages), test.Output);
        
        private static string ToName(ITestCase testCase) => testCase.TestMethod.Method.Name.Humanize();

        private static string ToTest(ITestCase testCase) => testCase.DisplayName;

        private static string ToExpected(SyntaxNode syntaxNode)
        {
            var assertExpression = syntaxNode.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault(invocationExpression => 
                    invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression &&
                    memberAccessExpression.Expression is IdentifierNameSyntax identifierName &&
                    identifierName.Identifier.Text == "Assert");

            if (assertExpression == null)
                return null;

            var memberAccessExpression = (MemberAccessExpressionSyntax)assertExpression.Expression;
            var assertionIdentifierName = (IdentifierNameSyntax)memberAccessExpression.Name;
            
            if (assertionIdentifierName.Identifier.Text == "True")
                return "true";
            
            if (assertionIdentifierName.Identifier.Text == "False")
                return "false";
            
            if (assertionIdentifierName.Identifier.Text == "Equal")
                return assertExpression.ArgumentList.Arguments[0].ToString();
            
            if (assertionIdentifierName.Identifier.Text == "InRange")
                return $">= {assertExpression.ArgumentList.Arguments[1]} && <= {assertExpression.ArgumentList.Arguments[2]}";
            
            return default;
        }
    }
}