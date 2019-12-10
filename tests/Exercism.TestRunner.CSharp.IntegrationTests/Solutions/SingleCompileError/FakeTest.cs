using Xunit;

public class FakeTest
{
    [Fact]
    public void Add_should_add_numbers() =>
        Assert.Equal(2, Fake.Add(1, 1));
}
