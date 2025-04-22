using System;
using System.Collections.Generic;

namespace ConsoleLineEditor.Tests.Utilities;

public sealed class DelegateTextCompletion : ITextCompletion
{
    private readonly Func<string, string, string, IEnumerable<string>?> _callback;

    public DelegateTextCompletion(Func<string, string, string, IEnumerable<string>?> callback)
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public IEnumerable<string>? GetCompletions(string context, string word, string suffix)
    {
        return _callback(context, word, suffix);
    }
}