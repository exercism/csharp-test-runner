using System;

public class FakeTests
{
    [Fact]
    public void Add_should_add_numbers()
    {
        if (1 is int)
        {
            Assert.Equal(2, Fake.Add(1, 1));    
        }
    }
}
