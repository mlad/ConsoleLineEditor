namespace ConsoleLineEditor.Commands.Cursor;

public sealed class PreviousWordCommand : LineEditorCommand
{
    public override void Execute(LineEditorContext context)
    {
        context.Buffer.MoveToPreviousWord();
    }
}