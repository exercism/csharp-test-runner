using System;

using Xunit;
using System.IO;

public class FakeTests : IDisposable
{
    private FileStream _file;
    
    public FakeTests()
    {
        _file = File.Open("tmp.txt", FileMode.Create);
    }
    
    [Fact]
    public void Add_should_add_numbers() => Assert.Equal(2, new Fake().Add(1, 1));

    public void Dispose()
    {
        _file.Dispose();
    }
}