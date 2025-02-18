using System;
using System.Diagnostics;

public static class Fake
{
    public static int Add(int x, int y)
    {
        Console.Out.Write("String ");
        Console.Write("without ");
        Console.Error.Write("params ");
        Console.Write("output");
        return x + y;
    }

    public static int Sub(int x, int y)
    {
        return x - y;
    }

    public static int Mul(int x, int y)
    {
        Console.WriteLine("String with params output");
        System.Console.WriteLine("Values used:");
        Console.WriteLine("{0}, {1}", 2, true);
        System.Console.Out.WriteLine("-----");
        return x * y;
    }
}