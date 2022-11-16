using Xunit;
using Exercism.Tests;
using System.Runtime.InteropServices;

public class FakeTests
{
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static string LondonTimeZone => IsWindows ? "GMT Standard Time" : "Europe/London";
    public static string NewYorkTimeZone => IsWindows ? "Eastern Standard Time" : "America/New_York";
    public static string ParisTimeZone => IsWindows ? "W. Europe Standard Time" : "Europe/Paris";

    [Fact]
    public void Can_Get_TimeZones() => Assert.NotEmpty(Fake.GetTimeZones());

    [Fact]
    public void Has_London_TimeZone()
        => Assert.Contains(Fake.GetTimeZones(), tz => tz.Id == LondonTimeZone);
    [Fact]
    public void Has_NewYork_TimeZone()
        => Assert.Contains(Fake.GetTimeZones(), tz => tz.Id == NewYorkTimeZone);
    [Fact]
    public void Has_Paris_TimeZone()
        => Assert.Contains(Fake.GetTimeZones(), tz => tz.Id == ParisTimeZone);
}