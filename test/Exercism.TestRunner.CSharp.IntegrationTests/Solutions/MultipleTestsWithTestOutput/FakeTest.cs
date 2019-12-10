using System;
using Xunit;
using Xunit.Abstractions;

public class FakeTest
{
    private readonly ITestOutputHelper output;

    public FakeTest(ITestOutputHelper output) =>
        this.output = output;

    [Fact]
    public void Add_should_add_numbers()
    {
        output.WriteLine("String without params output");
        Assert.Equal(2, Fake.Add(1, 1));
    }

    [Fact(Skip = "Remove to run test")]
    public void Sub_should_subtract_numbers() =>
        Assert.Equal(4, Fake.Sub(7, 3));

    [Fact(Skip = "Remove to run test")]
    public void Mul_should_multiply_numbers()
    {
        output.WriteLine("String with params output: {0}, {1}", 2, true);
        Assert.Equal(7, Fake.Mul(2, 3));
    }
}
