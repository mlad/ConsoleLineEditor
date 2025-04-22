using Spectre.Console;

namespace ConsoleLineEditor;

public interface ILineEditorPrompt
{
    (Markup Markup, int Margin) GetPrompt(ILineEditorState state, int line);
}