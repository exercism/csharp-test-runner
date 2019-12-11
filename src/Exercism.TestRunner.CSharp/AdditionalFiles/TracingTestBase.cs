using System;
using System.Diagnostics;
using Xunit.Abstractions;

public abstract class TracingTestBase : IDisposable
{
    private readonly TestOutputTraceListener _testOutputTraceListener;

    protected TracingTestBase(ITestOutputHelper output)
    {
        _testOutputTraceListener = new TestOutputTraceListener(output);

        Trace.Listeners.Clear();
        Trace.Listeners.Add(_testOutputTraceListener);
    }

    public void Dispose() =>
        _testOutputTraceListener.Dispose();
}