using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleLineEditor.Commands;
using ConsoleLineEditor.Internal;
using ConsoleLineEditor.Internal.Extensions;
using Spectre.Console;
using Spectre.Console.Advanced;

namespace ConsoleLineEditor;

public sealed class LineEditor : IHighlighterAccessor
{
    private readonly IInputSource _source;
    private readonly IServiceProvider? _provider;
    private readonly IAnsiConsole _console;
    private readonly LineEditorRenderer _renderer;
    private readonly LineEditorHistory _history;
    private readonly InputBuffer _input;

    public KeyBindings KeyBindings { get; }
    public bool MultiLine { get; init; }

    public ILineEditorPrompt Prompt { get; init; } = new LineEditorPrompt("[yellow]>[/]");
    public ITextCompletion? Completion { get; init; }
    public IHighlighter? Highlighter { get; init; }
    public ILineEditorHistory History => _history;

    public ILineDecorationRenderer? LineDecorationRenderer { get; init; }

    public LineEditor(IAnsiConsole? terminal = null, IInputSource? source = null, IServiceProvider? provider = null)
    {
        _console = terminal ?? AnsiConsole.Console;
        _source = source ?? new DefaultInputSource(_console);
        _provider = provider;
        _renderer = new LineEditorRenderer(_console, this);
        _history = new LineEditorHistory();
        _input = new InputBuffer(_source);

        KeyBindings = new KeyBindings();
        KeyBindings.AddDefault();
    }

    public static bool IsSupported(IAnsiConsole console)
    {
        ArgumentNullException.ThrowIfNull(console);

        return
            console.Profile.Out.IsTerminal &&
            console.Profile.Capabilities.Ansi &&
            console.Profile.Capabilities.Interactive;
    }

    public Task<string?> ReadLine(CancellationToken cancellationToken)
    {
        return ReadLine(string.Empty, cancellationToken);
    }

    public async Task<string?> ReadLine(string text, CancellationToken cancellationToken)
    {
        var cancelled = false;
        var state = new LineEditorState(Prompt, text);

        _history.Reset();
        _input.Initialize(KeyBindings);
        _renderer.Refresh(state);

        while (true)
        {
            var result = await ReadLine(state, cancellationToken).ConfigureAwait(false);

            if (result.Result == SubmitAction.Cancel)
            {
                cancelled = true;
                break;
            }
            else if (result.Result == SubmitAction.Submit)
            {
                break;
            }
            else if (result.Result == SubmitAction.PreviousHistory)
            {
                if (_history.MovePrevious(state) && !SetContent(state, _history.Current))
                {
                    continue;
                }
            }
            else if (result.Result == SubmitAction.NextHistory)
            {
                if (_history.MoveNext() && !SetContent(state, _history.Current))
                {
                    continue;
                }
            }
            else if (result.Result == SubmitAction.NewLine && MultiLine)
            {
                // Save and cut everything after current cursor position
                var content = state.Buffer.Content[state.Buffer.Position..];
                state.Buffer.Clear(state.Buffer.Position, state.Buffer.Length - state.Buffer.Position);

                // Add new line
                state.AddLine(content);
                state.Buffer.MoveHome();

                // Refresh
                var builder = new StringBuilder();
                builder.Append("\u001b[?25l"); // Hide cursor
                _renderer.AnsiBuilder.MoveDown(builder, state);
                _renderer.AnsiBuilder.BuildRefresh(builder, state);
                builder.Append("\u001b[?25h"); // Show cursor
                _console.WriteAnsi(builder.ToString());
            }
            else if (result.Result == SubmitAction.MoveUp && MultiLine)
            {
                MoveUp(state);
            }
            else if (result.Result == SubmitAction.MoveDown && MultiLine)
            {
                MoveDown(state);
            }
            else if (result.Result == SubmitAction.MoveFirst && MultiLine)
            {
                MoveFirst(state);
            }
            else if (result.Result == SubmitAction.MoveLast && MultiLine)
            {
                MoveLast(state);
            }
            else if (result.Result == SubmitAction.RemoveLine && MultiLine && state.LineIndex != 0)
            {
                // Insert current line text to the end of previous one
                var prevBuffer = state.GetBufferAt(state.LineIndex - 1);
                prevBuffer.MoveEnd();
                prevBuffer.Insert(state.Buffer.Content);

                state.RemoveLine();

                // Refresh
                var builder = new StringBuilder();
                _renderer.AnsiBuilder.BuildRemoveCurrentLine(builder, state);
                _console.WriteAnsi(builder.ToString());
            }
        }

        _renderer.RenderLine(state, cursorPosition: 0);

        // Move the cursor to the last line
        while (state.MoveDown())
        {
            _console.Cursor.MoveDown();
        }

        // Moving the cursor won't work here if we're at
        // the bottom of the screen, so let's insert a new line.
        _console.WriteLine();

        // Return all the lines
        return cancelled ? null : state.Text;
    }

    private async Task<(LineBuffer Buffer, SubmitAction Result)> ReadLine(
        LineEditorState state,
        CancellationToken cancellationToken)
    {
        var provider = new DefaultServiceProvider(_provider);
        provider.RegisterOptional<ITextCompletion, ITextCompletion>(Completion);
        var context = new LineEditorContext(state.Buffer, provider);

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return (state.Buffer, SubmitAction.Cancel);
            }

            // Get command
            var command = default(LineEditorCommand);
            var key = await _input.ReadKey(MultiLine, cancellationToken).ConfigureAwait(false);
            if (key != null)
            {
                if (key.Value.KeyChar != 0 && !char.IsControl(key.Value.KeyChar))
                {
                    command = new InsertCommand(key.Value.KeyChar);
                }
                else
                {
                    command = KeyBindings.GetCommand(key.Value.Key, key.Value.Modifiers);
                }
            }

            // Execute command
            if (command != null)
            {
                context.Execute(command);
            }

            // Time to exit?
            if (context.Result != null)
            {
                return (state.Buffer, context.Result.Value);
            }

            // Render the line
            _renderer.RenderLine(state);
            LineDecorationRenderer?.RenderLineDecoration(state.Buffer);
        }
    }

    private void MoveUp(LineEditorState state)
    {
        Move(state, () =>
        {
            if (state.MoveUp())
            {
                _console.Cursor.MoveUp();
            }
        });
    }

    private void MoveDown(LineEditorState state)
    {
        Move(state, () =>
        {
            if (state.MoveDown())
            {
                _console.Cursor.MoveDown();
            }
        });
    }

    private void MoveFirst(LineEditorState state)
    {
        Move(state, () =>
        {
            while (state.MoveUp())
            {
                _console.Cursor.MoveUp();
            }
        });
    }

    private void MoveLast(LineEditorState state)
    {
        Move(state, () =>
        {
            while (state.MoveDown())
            {
                _console.Cursor.MoveDown();
            }
        });
    }

    private void Move(LineEditorState state, Action action)
    {
        using (_console.HideCursor())
        {
            if (state.LineCount > _console.Profile.Height)
            {
                // Get the current position
                var position = state.Buffer.Position;

                // Refresh everything
                action();
                _renderer.Refresh(state);

                // Re-render the current line at the correct position
                state.Buffer.Move(position);
                _renderer.RenderLine(state);
            }
            else
            {
                // Get the current position
                var position = state.Buffer.Position;

                // Reset the line
                _renderer.RenderLine(state, cursorPosition: 0);
                action();

                // Render the current line at the correct position
                state.Buffer.Move(position);
                _renderer.RenderLine(state);
            }
        }
    }

    private bool SetContent(LineEditorState state, IList<LineBuffer>? lines)
    {
        // Nothing to set?
        if (lines == null || lines.Count == 0)
        {
            return false;
        }

        var builder = new StringBuilder();

        // Clearing the current lines will
        // move the cursor to the top.
        _renderer.AnsiBuilder.BuildClear(builder, state);

        // Remove all lines
        state.RemoveAllLines();

        // Hide the cursor
        builder.Append("\u001b[?25l");

        // Add all the lines
        foreach (var line in lines)
        {
            state.AddLine(line.Content);
        }

        // Make room for all the lines
        var first = true;
        for (var i = 0; i < lines.Count; i++)
        {
            var shouldAddNewLine = true;
            if (first)
            {
                shouldAddNewLine = false;
                first = false;
            }

            if (shouldAddNewLine)
            {
                _renderer.AnsiBuilder.MoveDown(builder, state);
            }
        }

        _renderer.AnsiBuilder.BuildRefresh(builder, state);

        // Show the cursor again
        builder.Append("\u001b[?25h");

        // Flush
        _console.WriteAnsi(builder.ToString());
        return true;
    }
}