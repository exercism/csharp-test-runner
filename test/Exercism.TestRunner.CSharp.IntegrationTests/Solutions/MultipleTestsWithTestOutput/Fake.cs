using System;
using System.Diagnostics;

public static class Fake
{
    public static int Add(int x, int y)
    {
        Trace.Write("String");
        Trace.Write(" ");
        Trace.WriteLine("without params output");
        return x + y;
    }

    public static int Sub(int x, int y)
    {
        return x - y;
    }

    public static int Mul(int x, int y)
    {
        Debug.WriteLine("String with params output: {0}, {1}", 2, true);
        return x * y;
    }
}