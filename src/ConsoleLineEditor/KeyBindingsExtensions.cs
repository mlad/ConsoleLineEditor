using System;
using ConsoleLineEditor.Commands;
using ConsoleLineEditor.Commands.Cursor;
using ConsoleLineEditor.Internal;

namespace ConsoleLineEditor;

public static class KeyBindingsExtensions
{
    public static void AddDefault(this KeyBindings bindings)
    {
        bindings.Add(ConsoleKey.Tab, () => new AutoCompleteCommand(AutoComplete.Next));
        bindings.Add(ConsoleKey.Tab, ConsoleModifiers.Control, () => new AutoCompleteCommand(AutoComplete.Previous));

        bindings.Add<BackspaceCommand>(ConsoleKey.Backspace);
        bindings.Add<DeleteCommand>(ConsoleKey.Delete);
        bindings.Add<MoveHomeCommand>(ConsoleKey.Home);
        bindings.Add<MoveEndCommand>(ConsoleKey.End);
        bindings.Add<MoveUpCommand>(ConsoleKey.UpArrow);
        bindings.Add<MoveDownCommand>(ConsoleKey.DownArrow);
        bindings.Add<MoveLeftCommand>(ConsoleKey.LeftArrow);
        bindings.Add<MoveRightCommand>(ConsoleKey.RightArrow);
        bindings.Add<PreviousWordCommand>(ConsoleKey.LeftArrow, ConsoleModifiers.Control);
        bindings.Add<NextWordCommand>(ConsoleKey.RightArrow, ConsoleModifiers.Control);
        bindings.Add<PreviousHistoryCommand>(ConsoleKey.PageUp);
        bindings.Add<NextHistoryCommand>(ConsoleKey.PageDown);
        bindings.Add<SubmitCommand>(ConsoleKey.Enter);
        bindings.Add<NewLineCommand>(ConsoleKey.Enter, ConsoleModifiers.Shift);

        // bindings.Add<MoveFirstLineCommand>(ConsoleKey.PageUp);
        // bindings.Add<MoveLastLineCommand>(ConsoleKey.PageDown);
    }

    public static void Add<TCommand>(this KeyBindings bindings, ConsoleKey key, ConsoleModifiers? modifiers = null)
        where TCommand : LineEditorCommand, new()
    {
        ArgumentNullException.ThrowIfNull(bindings);

        bindings.Add(new KeyBinding(key, modifiers), () => new TCommand());
    }

    public static void Add<TCommand>(this KeyBindings bindings, ConsoleKey key, Func<TCommand> func)
        where TCommand : LineEditorCommand
    {
        ArgumentNullException.ThrowIfNull(bindings);

        bindings.Add(new KeyBinding(key), () => func());
    }

    public static void Add<TCommand>(this KeyBindings bindings, ConsoleKey key, ConsoleModifiers modifiers, Func<TCommand> func)
        where TCommand : LineEditorCommand
    {
        ArgumentNullException.ThrowIfNull(bindings);

        bindings.Add(new KeyBinding(key, modifiers), () => func());
    }

    public static void Remove(this KeyBindings bindings, ConsoleKey key, ConsoleModifiers? modifiers = null)
    {
        ArgumentNullException.ThrowIfNull(bindings);

        bindings.Remove(new KeyBinding(key, modifiers));
    }
}