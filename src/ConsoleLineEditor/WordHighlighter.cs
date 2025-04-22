using System;
using System.Collections.Generic;
using ConsoleLineEditor.Internal;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleLineEditor;

public sealed class WordHighlighter : IHighlighter
{
    private readonly Dictionary<string, Style> _words;

    public WordHighlighter(StringComparer? comparer = null)
    {
        _words = new Dictionary<string, Style>(comparer ?? StringComparer.OrdinalIgnoreCase);
    }

    public WordHighlighter AddWord(string word, Style style)
    {
        _words[word] = style;
        return this;
    }

    IRenderable IHighlighter.BuildHighlightedText(string text)
    {
        var paragraph = new Paragraph();
        foreach (var token in StringTokenizer.Tokenize(text))
        {
            paragraph.Append(token, _words.GetValueOrDefault(token));
        }

        return paragraph;
    }
}