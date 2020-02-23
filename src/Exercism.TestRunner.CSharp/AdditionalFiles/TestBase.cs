using System;
using System.IO;
using System.Diagnostics;
using Xunit.Abstractions;

public abstract class TestBase : IDisposable
{
    private const int MaximumLength = 500;
    
    private readonly ITestOutputHelper _testOutput;
    private readonly StringWriter _stringWriter;

    protected TestBase(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
        _stringWriter = new StringWriter();
            
        Console.SetOut(_stringWriter);
        Console.SetError(_stringWriter);

        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    public void Dispose()
    {
        var output = _stringWriter.ToString();

        if (output.Length > MaximumLength)
            _testOutput.WriteLine($"{output.Substring(0, MaximumLength)}\nOutput was truncated. Please limit to {MaximumLength} chars.");
        else
            _testOutput.WriteLine(output);
    }
}