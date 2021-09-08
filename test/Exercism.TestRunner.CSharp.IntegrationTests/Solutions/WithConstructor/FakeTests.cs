using Xunit;
using System.Globalization;
using System.Threading;

public class FakeTests
{
    public FakeTests()
    {
        var enUsCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = enUsCulture;
        Thread.CurrentThread.CurrentUICulture = enUsCulture;
    }
    
    [Fact]
    public void Add_should_add_numbers() => Assert.Equal(2, new Fake().Add(1, 1));
}