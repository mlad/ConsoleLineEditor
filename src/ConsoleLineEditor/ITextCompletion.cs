using System.Collections.Generic;

namespace ConsoleLineEditor;

public interface ITextCompletion
{
    public IEnumerable<string>? GetCompletions(string prefix, string word, string suffix);
}