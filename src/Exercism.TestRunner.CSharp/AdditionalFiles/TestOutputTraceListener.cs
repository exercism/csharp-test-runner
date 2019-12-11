using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

public class TestOutputTraceListener : TraceListener
{
    private const int MaximumLength = 500;

    private readonly ITestOutputHelper _output;
    private readonly StringBuilder _sb;

    public TestOutputTraceListener(ITestOutputHelper output)
    {
        _output = output;
        _sb = new StringBuilder();
    }

    public override void Write(string message) =>
        _sb.Append(message);

    public override void WriteLine(string message) =>
        _sb.AppendLine(message);

    protected override void Dispose(bool disposing)
    {
        _output.WriteLine(TraceOutput);
        base.Dispose(disposing);
    }

    private string TraceOutput =>
        _sb.Length > MaximumLength
            ? _sb.ToString().Substring(0, MaximumLength) + $"\nOutput was truncated. Please limit to {MaximumLength} chars."
            : _sb.ToString();
}