namespace ConsoleLineEditor.Commands.Cursor;

public sealed class MoveUpCommand : LineEditorCommand
{
    public override void Execute(LineEditorContext context)
    {
        context.Submit(SubmitAction.MoveUp);
    }
}