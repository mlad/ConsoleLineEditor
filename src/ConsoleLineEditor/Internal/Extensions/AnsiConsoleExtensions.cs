using System;
using Spectre.Console;

namespace ConsoleLineEditor.Internal.Extensions;

internal static class AnsiConsoleExtensions
{
    public static IDisposable HideCursor(this IAnsiConsole console)
    {
        ArgumentNullException.ThrowIfNull(console);
        return new CursorHider(console);
    }

    private sealed class CursorHider : IDisposable
    {
        private readonly IAnsiConsole _console;

        public CursorHider(IAnsiConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _console.Cursor.Hide();
        }

        ~CursorHider()
        {
            throw new InvalidOperationException("CursorHider: Dispose was never called");
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _console.Cursor.Show();
        }
    }
}