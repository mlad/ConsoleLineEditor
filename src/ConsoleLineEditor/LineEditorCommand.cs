namespace ConsoleLineEditor;

public abstract class LineEditorCommand
{
    public abstract void Execute(LineEditorContext context);
}