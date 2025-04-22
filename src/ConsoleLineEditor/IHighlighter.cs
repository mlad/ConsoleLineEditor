using Spectre.Console.Rendering;

namespace ConsoleLineEditor;

public interface IHighlighter
{
    IRenderable BuildHighlightedText(string text);
}