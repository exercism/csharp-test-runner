using Xunit;
public class FakeTests
{
    [Fact]
    public void Add_should_add_numbers() => FakeTestsHelper.AssertAdd(2, 1, 1);
}