using Xunit;
using Xunit.Abstractions;

public class FakeTest
{
    [Fact]
    public void Add_should_add_numbers() =>
        Assert.Equal(3, Fake.Add(1, 1));
}
