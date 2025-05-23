namespace ConsoleLineEditor.Commands;

public sealed class SubmitCommand : LineEditorCommand
{
    public override void Execute(LineEditorContext context)
    {
        context.Submit(SubmitAction.Submit);
    }
}