public static class StringExtensions
{
    public static string UseUnixNewlines(this string str) =>
        str.Replace("\r\n", "\n");

    public static string NullIfEmpty(this string str) =>
        str == string.Empty ? null : str;
}