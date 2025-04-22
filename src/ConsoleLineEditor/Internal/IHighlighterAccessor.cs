namespace ConsoleLineEditor.Internal;

internal interface IHighlighterAccessor
{
    IHighlighter? Highlighter { get; }
}