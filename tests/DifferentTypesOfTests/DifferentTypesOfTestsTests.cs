using Xunit;
using FsCheck;
using FsCheck.Fluent;

public class FakeTests
{
    [Fact]
    public void Identity() =>
        Assert.Equal(2, Fake.Identity(2));
    
    [Fact(Skip = "Remove this Skip property to run this test")]
    public void Add_should_add_numbers() =>
        Assert.Equal(2, Fake.Add(1, 1));
    
    [Fact(Skip = "Remove this Skip property to run this test")]
    public void Div_should_divide_numbers() =>
        Prop.ForAll<PositiveInt>(i => Fake.Div(i.Get, i.Get) == 1)
            .QuickCheckThrowOnFailure();
}
