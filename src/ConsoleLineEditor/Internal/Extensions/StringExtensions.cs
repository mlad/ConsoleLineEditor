namespace ConsoleLineEditor.Internal.Extensions;

internal static class StringExtensions
{
    public static string NormalizeNewLines(this string? text)
    {
        text = text?.Replace("\r\n", "\n");
        text ??= string.Empty;
        return text;
    }
}