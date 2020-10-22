public static class StringExtensions
{
    public static string UseUnixNewlines(this string str) =>
        str.Replace("\r\n", "\n");
}