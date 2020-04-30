public static class Fake
{
    public static int Add(int x, int y) => x + y;

    public static int Sub(int x, int y) => x - y;

    public static int Mul(int x, int y) => x * y;
}

public static class Foo
{
    public static string Upper(string str) => str.ToUpper();

    public static string Lower(string str) => str.ToLower();

}