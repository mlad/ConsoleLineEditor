using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ConsoleLineEditor;

public static class ITextCompletionExtensions
{
    public static bool TryGetCompletions(
        this ITextCompletion completion,
        string prefix, string word, string suffix,
        [NotNullWhen(true)] out string[]? result)
    {
        var completions = completion.GetCompletions(prefix, word, suffix)?.ToArray();
        if (completions == null || completions.Length == 0)
        {
            result = null;
            return false;
        }

        result = completions;
        return true;
    }
}