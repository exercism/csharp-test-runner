using Xunit;

public class FakeTests
{
    [Fact]
    public void Add_should_add_numbers() =>
        Assert.Equal(3, Fake.Add(1, 1));

    [Fact(Skip = "Remove to run test")]
    public void Sub_should_subtract_numbers() =>
        Assert.Equal(1, Fake.Sub(2, 1));

    [Fact(Skip = "Remove to run test")]
    public void Mul_should_multiply_numbers() =>
        Assert.Equal(7, Fake.Mul(2, 3));
}
