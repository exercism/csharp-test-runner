using Xunit;

public static class FakeTestsHelper
{
    public static void AssertAdd(int expected, int x, int y) =>
        Assert.Equal(expected, Fake.Add(x, y));
}
