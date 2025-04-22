namespace ConsoleLineEditor.Commands;

public sealed class NewLineCommand : LineEditorCommand
{
    public override void Execute(LineEditorContext context)
    {
        context.Submit(SubmitAction.NewLine);
    }
}