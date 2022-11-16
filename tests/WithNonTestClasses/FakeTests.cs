using Xunit;
public class FakeTests
{
    private NumberComparer _numberComparer = new();
    
    [Fact]
    public void Add_should_add_numbers() => _numberComparer.Compare(2, FakeHelper.AddOne(1));

    private class NumberComparer
    {
        public void Compare(int expected, int actual) => Assert.Equal(expected, actual);
    }
}

public static class FakeHelper
{
    public static int AddOne(int x) => Fake.Add(x, 1);
}