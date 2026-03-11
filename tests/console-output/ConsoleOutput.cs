using System.Diagnostics;

public static class Fake
{
    public static int Add(int x, int y)
    {
        Console.WriteLine($"Console: adding {x} and {y}");
        Trace.WriteLine($"Trace: adding {x} and {y}");
        return x + y;
    } 
}
