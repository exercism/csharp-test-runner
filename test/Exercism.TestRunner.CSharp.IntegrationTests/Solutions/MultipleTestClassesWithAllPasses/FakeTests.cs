using Xunit;

public class FakeTests
{
    [Fact]
    public void Add_should_add_numbers() =>
        Assert.Equal(2, Fake.Add(1, 1));

    [Fact(Skip = "Remove to run test")]
    public void Sub_should_subtract_numbers() =>
        Assert.Equal(4, Fake.Sub(7, 3));

    [Fact(Skip = "Remove to run test")]
    public void Mul_should_multiply_numbers() =>
        Assert.Equal(6, Fake.Mul(2, 3));
}

public class FooTest
{
    [Fact(Skip = "Remove to run test")]
    public void Upper_should_uppercase_string() =>
        Assert.Equal("HELLO", Foo.Upper("hello"));

    [Fact(Skip = "Remove to run test")]
    public void Lower_should_lowercase_string() =>
        Assert.Equal("hello", Foo.Lower("HELLO"));
}
