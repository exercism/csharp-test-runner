using System;
using System.Collections.Generic;
using System.Linq;

using Humanizer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit.Runners;

namespace Exercism.TestRunner.CSharp
{
    internal static class TestResultParser
    {
        public static TestResult[] FromTests(IEnumerable<TestInfo> tests, SyntaxTree testsSyntaxTree)
        {
            var testMethods =
                testsSyntaxTree
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .ToArray();

            return tests
                    .Select(test => (test: test, testMethod: test.TestMethod(testMethods)))
                    .OrderBy(testAndMethod => Array.IndexOf(testMethods, testAndMethod.testMethod))
                    .Select(testAndMethod => FromTest(testAndMethod.test, testAndMethod.testMethod))
                    .ToArray();
        }

        private static TestResult FromTest(TestInfo test, MethodDeclarationSyntax testMethod) =>
            test switch
            {
                TestFailedInfo failedTest => FromFailedTest(failedTest, testMethod),
                TestPassedInfo passedTest => FromPassedTest(passedTest, testMethod),
                _ => throw new ArgumentOutOfRangeException(nameof(test))
            };

        private static TestResult FromFailedTest(TestFailedInfo info, MethodDeclarationSyntax testMethod) =>
            new()
            {
                Name = info.Name(),
                Status = TestStatus.Fail,
                Message = info.Message(),
                Output = info.Output(),
                TaskId = testMethod.TaskId(),
                TestCode = testMethod.TestCode()
            };

        private static TestResult FromPassedTest(TestPassedInfo info, MethodDeclarationSyntax testMethod) =>
            new()
            {
                Name = info.Name(),
                Status = TestStatus.Pass,
                Output = info.Output(),
                TaskId = testMethod.TaskId(),
                TestCode = testMethod.TestCode()
            };

        private static MethodDeclarationSyntax TestMethod(this TestInfo testInfo, IEnumerable<MethodDeclarationSyntax> methodDeclarations) =>
            methodDeclarations.Single(method =>
                method.Identifier.Text == testInfo.MethodName &&
                method.Parent is ClassDeclarationSyntax classDeclaration &&
                classDeclaration.Identifier.Text == testInfo.TypeName);

        private static string Name(this TestInfo testInfo) =>
            testInfo.MethodName.Humanize();

        private static string Message(this TestFailedInfo testInfo) =>
            testInfo.ExceptionMessage.UseUnixNewlines()?.Trim();

        private static string Output(this TestExecutedInfo testInfo) =>
            testInfo.Output.UseUnixNewlines().Trim().NullIfEmpty();

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
}