namespace ConsoleLineEditor.Commands.Cursor;

public sealed class NextWordCommand : LineEditorCommand
{
    public override void Execute(LineEditorContext context)
    {
        context.Buffer.MoveToNextWord();
    }
}