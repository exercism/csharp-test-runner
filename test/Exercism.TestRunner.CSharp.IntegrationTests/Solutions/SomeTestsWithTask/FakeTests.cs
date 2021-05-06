using Xunit;
using Exercism.Tests;

public class FakeTests
{
    [Fact]
    [Task(1)]
    public void Add_should_add_numbers() => Assert.Equal(2, Fake.Add(1, 1));

    [Fact]
    [Task(2)]
    public void Sub_should_subtract_numbers() => Assert.Equal(4, Fake.Sub(7, 3));

    [Fact]
    public void Mul_should_multiply_numbers() => Assert.Equal(6, Fake.Mul(2, 3));
}