using Xunit;

public class FakeExpressionBodies
{
    [Fact]
    public void Test_Using_Assert_Equal_In_Expression_Body_With_Expected_Int() =>
        Assert.Equal(-5, Fake.Invert(5));

    [Fact]
    public void Test_Using_Assert_Equal_In_Expression_Body_With_Expected_String() =>
        Assert.Equal("Number: 7", Fake.Describe(7));
    
    [Fact]
    public void Test_Using_Assert_In_Range_In_Expression_Body_With_Expected_Int() =>
        Assert.InRange(Fake.Invert(-5), 3, 7);
    
    [Fact]
    public void Test_Using_Assert_True_In_Expression_Body() =>
        Assert.True(Fake.Positive(1));
    
    [Fact]
    public void Test_Using_Assert_False_In_Expression_Body() =>
        Assert.False(Fake.Positive(-2));
}

public class FakeSingleLineStatements
{
    [Fact]
    public void Test_Using_Assert_Equal_In_Single_Line_Statement_With_Expected_Int()
    {
        Assert.Equal(-5, Fake.Invert(5));
    }
    
    [Fact]
    public void Test_Using_Assert_Equal_In_Single_Line_Statement_With_Expected_String()
    {
        Assert.Equal("Number: 7", Fake.Describe(7));
    }
    
    [Fact]
    public void Test_Using_Assert_In_Range_In_Single_line_With_Expected_Int()
    {
        Assert.InRange(Fake.Invert(-5), 3, 7);
    }
    
    [Fact]
    public void Test_Using_Assert_True_In_Single_Line_Statement()
    {
        Assert.True(Fake.Positive(1));
    }
    
    [Fact]
    public void Test_Using_Assert_False_In_Single_Line_Statement()
    {
        Assert.False(Fake.Positive(-2));
    }
}

public class FakeMultiLineStatements
{
    [Fact]
    public void Test_Using_Assert_Equal_In_Multi_Line_Statement_With_Expected_Int()
    {
        var fake = new Fake(3);
        fake.Foo();
        Assert.Equal(-3, fake.Invert());
    }
    
    [Fact]
    public void Test_Using_Assert_Equal_In_Multi_Line_Statement_With_Expected_String()
    {
        var fake = new Fake(9);
        fake.Foo();
        Assert.Equal("Number: 9", fake.Describe());
    }
    
    [Fact]
    public void Test_Using_Assert_In_Range_In_Single_line_With_Expected_Int()
    {
        var fake = new Fake(9);
        fake.Foo();
        Assert.InRange(fake.Invert(), 3, 7);
    }
    
    [Fact]
    public void Test_Using_Assert_True_In_Multi_Line_Statement()
    {
        var fake = new Fake(6);
        fake.Foo();
        Assert.True(fake.Positive());
    }
    
    [Fact]
    public void Test_Using_Assert_False_In_Multi_Line_Statement()
    {
        var fake = new Fake(-8);
        fake.Foo();
        Assert.False(fake.Positive());
    }
}