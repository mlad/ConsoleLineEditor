﻿namespace ConsoleLineEditor.Commands.Cursor;

public sealed class MoveLastLineCommand : LineEditorCommand
{
    public override void Execute(LineEditorContext context)
    {
        context.Submit(SubmitAction.MoveLast);
    }
}