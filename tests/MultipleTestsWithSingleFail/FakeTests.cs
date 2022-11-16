using Xunit;
using Xunit.Sdk;
using System.Collections.Generic;
using System.Linq;

public class FakeTests
{
    [Fact]
    public void Add_should_add_numbers() =>
        Assert.Equal(2, Fake.Add(1, 1));

    [Fact]
    public void Sub_should_subtract_numbers() =>
        Assert.Equal(1, Fake.Sub(2, 1));

    [Fact]
    public void Mul_should_multiply_numbers() =>
        Assert.Equal(5, Fake.Mul(2, 3));
}