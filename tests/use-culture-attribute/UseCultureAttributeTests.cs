using Exercism.Tests;

public class FakeTests
{
    [Fact]
    [UseCulture]
    public void Use_invariant_culture() => Assert.Equal("¤1,000.00", Fake.Format(1000));
    
    [Fact]
    [UseCulture("nl-NL")]
    public void Use_dutch_culture() => Assert.Equal("€ 1.000,00", Fake.Format(1000));
    
    [Fact]
    [UseCulture("en-US")]
    public void Use_us_culture() => Assert.Equal("$1,000.00", Fake.Format(1000));
}