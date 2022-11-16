using System;

using Xunit;
using FsCheck.Xunit;
using FsCheck;

public class FakeTests
{
    [Fact]
    public void Identity() =>
        Assert.Equal(2, Fake.Identity(2));
    
    [Fact(Skip = "Remove this Skip property to run this test")]
    public void Add_should_add_numbers() =>
        Assert.Equal(2, Fake.Add(1, 1));
    
    [Theory(Skip = "Remove this Skip property to run this test")]
    [InlineData(4, 7, 3)]
    public void Sub_should_subtract_numbers(int expected, int x, int y) =>
        Assert.Equal(expected, Fake.Sub(x, y));
    
    [CustomPropertyAttribute(Skip = "Remove this Skip property to run this test")]
    public void Mul_should_multiply_numbers(int x, int y) =>
        Assert.Equal(x * y, Fake.Mul(x, y));
    
    [Property(Skip = "Remove this Skip property to run this test")]
    public Property Div_should_divide_numbers(int x) =>
        Prop.Throws<DivideByZeroException, int>(new Lazy<int>(() => x / 0));
}

public class CustomPropertyAttribute : PropertyAttribute
{
}
