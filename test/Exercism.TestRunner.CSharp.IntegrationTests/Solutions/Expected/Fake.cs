public class Fake
{
    private readonly int x; 
    
    public Fake(int x)
    {
        this.x = x;
    }
    
    public int Invert() => -x;

    public bool Positive() => x >= 0;

    public string Describe() => $"Number: {x}";
    
    public void Foo()
    {
    }
    
    public static int Invert(int x) => -x;

    public static bool Positive(int x) => x >= 0;

    public static string Describe(int x) => $"Number: {x}";
}