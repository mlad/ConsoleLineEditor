namespace ConsoleLineEditor.Commands.Cursor;

public sealed class MoveHomeCommand : LineEditorCommand
{
    public override void Execute(LineEditorContext context)
    {
        context.Buffer.MoveHome();
    }
}