public class FakeTests
{
    [Fact]
    public void Single_assertion_as_expression_body() =>
        Assert.Equal(2, Fake.Add(1, 1));

    [Fact]
    public void Single_assertion_as_single_statement_block()
    {
        Assert.Equal(4, Fake.Sub(7, 3));   
    }

    [Fact]
    public void Single_assertion_with_non_assertion_statement_block()
    {
        var x = Fake.Sub(7, 3);
        Assert.Equal(4, x);   
    }

    [Fact]
    public void Multiple_assertions_as_block()
    {
        Assert.Equal(6, Fake.Mul(2, 3));
        Assert.Equal(8, Fake.Mul(2, 4));
    }

    [Fact]
    public void Multiple_assertions_with_non_assertion_statements_block()
    {
        var x = Fake.Mul(2, 3);
        var y = Fake.Mul(2, 4);
        Assert.Equal(6, x);
        Assert.Equal(8, y);
    }
}