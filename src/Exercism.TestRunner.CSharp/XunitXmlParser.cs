using System.Xml;
using System.Xml.Linq;

namespace Exercism.TestRunner.CSharp;

/// <summary>
/// Reads the XML report written by an xUnit v3 test executable (its -xml option).
/// Returns null when the report is missing or shows anything other than plain passes and failures,
/// so that the caller can fall back to running the tests through VSTest.
/// </summary>
internal static class XunitXmlParser
{
    public static List<RawTestResult>? Parse(string reportFilePath)
    {
        if (!File.Exists(reportFilePath))
            return null;

        // XML parsing turns a carriage return into a newline, so the original text could not be recovered.
        var reportText = File.ReadAllText(reportFilePath);
        if (reportText.Contains('\r'))
            return null;

        XDocument document;
        try
        {
            document = XDocument.Parse(reportText, LoadOptions.PreserveWhitespace);
        }
        catch (XmlException)
        {
            return null;
        }

        var assemblies = document.Root?.Elements("assembly").ToArray() ?? [];
        if (assemblies.Length != 1 || (string?)assemblies[0].Attribute("errors") != "0")
            return null;

        var results = new List<RawTestResult>();
        foreach (var test in document.Descendants("test"))
        {
            var name = (string?)test.Attribute("name");
            var outcome = (string?)test.Attribute("result") switch
            {
                "Pass" => "Passed",
                "Fail" => "Failed",
                _ => null
            };
            if (name is null || outcome is null)
                return null;

            var escapedMessage = test.Element("failure")?.Element("message")?.Value;
            var message = escapedMessage is null ? null : Unescape(escapedMessage);
            if (escapedMessage is not null && message is null)
                return null;

            results.Add(new RawTestResult(name, outcome, test.Element("output")?.Value, message));
        }

        return results;
    }

    // The XML report escapes failure messages (xUnit's ExecutionSink.XmlEscape): a backslash, a double
    // quote, \t, \r, \n, \0, \a, \b, \f and \v get a backslash, and other control characters and lone surrogates become
    // \xHH or \xHHHH. The VSTest report keeps tabs and newlines, and writes other control characters,
    // \uFFFE and \uFFFF as the text \uHHHH. This converts one form into the other, and returns null for
    // anything it does not recognise. That includes lone surrogates, which have not been checked, and the
    // escape character, because VSTest removes ANSI escape codes from messages.
    private static string? Unescape(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            var current = value[index];
            if (current != '\\')
            {
                builder.Append(current);
                continue;
            }

            if (++index >= value.Length)
                return null;

            switch (value[index])
            {
                case '\\': builder.Append('\\'); break;
                case '"': builder.Append('"'); break;
                case 't': builder.Append('\t'); break;
                case 'n': builder.Append('\n'); break;
                case '0': builder.Append("\\u0000"); break;
                case 'a': builder.Append("\\u0007"); break;
                case 'b': builder.Append("\\u0008"); break;
                case 'f': builder.Append("\\u000c"); break;
                case 'v': builder.Append("\\u000b"); break;
                case 'x':
                    // Control characters use two digits, and lone surrogates, \uFFFE and \uFFFF use four.
                    if (TryParseHex(value, index + 1, 4, out var wide) && wide >= 0xFFFE)
                    {
                        builder.Append($"\\u{wide:x4}");
                        index += 4;
                    }
                    else if (TryParseHex(value, index + 1, 2, out var narrow) && narrow < 0x20 && narrow != 0x1b)
                    {
                        builder.Append($"\\u{narrow:x4}");
                        index += 2;
                    }
                    else
                    {
                        return null;
                    }
                    break;
                default:
                    return null;
            }
        }

        return builder.ToString();
    }

    private static bool TryParseHex(string value, int start, int length, out int result)
    {
        result = 0;
        return start + length <= value.Length &&
               int.TryParse(value.AsSpan(start, length), System.Globalization.NumberStyles.AllowHexSpecifier, null, out result);
    }
}
