using Xunit;
public class FooTests
{
    [Fact]
    public void Add_should_add_numbers() => Assert.Equal(2, Foo.Add(1, 1));
}