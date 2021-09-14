using System;
using System.Collections.Generic;

public static class Fake
{
    public static IEnumerable<TimeZoneInfo> GetTimeZones() => TimeZoneInfo.GetSystemTimeZones();
}
