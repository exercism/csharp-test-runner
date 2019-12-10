using Xunit;
using Xunit.Abstractions;

public class FakeTest
{
    [Fact]
    public void Add_should_add_numbers() =>
        Assert.Equal(2, Fake.Add(1, 1));

    [Fact]
    public void Sub_should_sub_numbers() =>
        Assert.Equal(2, Fake.Sub(3, 1));
}
