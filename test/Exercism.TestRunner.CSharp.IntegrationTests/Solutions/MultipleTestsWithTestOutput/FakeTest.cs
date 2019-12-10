using System;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;

public class TestOutputTraceListener : TraceListener
{
    private readonly ITestOutputHelper _output;
    private readonly StringBuilder _sb;

    public TestOutputTraceListener(ITestOutputHelper output)
    {
        _output = output;
        _sb = new StringBuilder(500, 500);
    }

    public override void Write(string message) =>
        _sb.Append(message);

    public override void WriteLine(string message) =>
        _sb.AppendLine(message);

    protected override void Dispose(bool disposing)
    {
        _output.WriteLine(_sb.ToString());
        base.Dispose(disposing);
    }
}

public class FakeTest : IDisposable
{
    private readonly TestOutputTraceListener _testOutputTraceListener;

    public FakeTest(ITestOutputHelper output)
    {
        _testOutputTraceListener = new TestOutputTraceListener(output);

        Trace.Listeners.Clear();
        Trace.Listeners.Add(_testOutputTraceListener);
    }

    public void Dispose()
    {
        _testOutputTraceListener.Dispose();
    }

    [Fact]
    public void Add_should_add_numbers() =>
        Assert.Equal(2, Fake.Add(1, 1));

    [Fact(Skip = "Remove to run test")]
    public void Sub_should_subtract_numbers() =>
        Assert.Equal(4, Fake.Sub(7, 3));

    [Fact(Skip = "Remove to run test")]
    public void Mul_should_multiply_numbers() =>
        Assert.Equal(7, Fake.Mul(2, 3));


    
}
