using Xunit;
public class FakeTest : System.IDisposable
{
    [Fact]
    public void Add_should_add_numbers() => Assert.Equal(2, Fake.Add(1, 1));
    [Fact]
    public void Sub_should_subtract_numbers() => Assert.Equal(4, Fake.Sub(7, 3));
    [Fact]
    public void Mul_should_multiply_numbers() => Assert.Equal(6, Fake.Mul(2, 3));
    Xunit.Abstractions.ITestOutputHelper _testOutput;
    System.IO.StringWriter _stringWriter;
    public FakeTest(Xunit.Abstractions.ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
        _stringWriter = new System.IO.StringWriter();
        System.Console.SetOut(_stringWriter);
        System.Console.SetError(_stringWriter);
        System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
    }

    public void Dispose()
    {
        var output = _stringWriter.ToString();
        if (output.Length > 500)
        {
            output = output.Substring(0, 450) + "\r\n\r\n    Output was truncated. Please limit to 500 chars.";
        }

        _testOutput.WriteLine(output);
    }
}