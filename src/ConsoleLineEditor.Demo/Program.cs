using ConsoleLineEditor;
using Spectre.Console;

if (!LineEditor.IsSupported(AnsiConsole.Console))
{
    AnsiConsole.MarkupLine("The terminal does not support ANSI codes, or it isn't a terminal.");
}

var text = "Hello, and welcome to ConsoleLineEditor!\nPress SHIFT+ENTER to insert a new line\nPress ENTER to submit";

// Create editor
var editor = new LineEditor
{
    MultiLine = true,
    Prompt = new LineNumberPrompt(new Style(foreground: Color.Yellow, background: Color.Black)),
    Completion = new TestCompletion(),
    Highlighter = new WordHighlighter()
        .AddWord("git", new Style(foreground: Color.Yellow))
        .AddWord("code", new Style(foreground: Color.Yellow))
        .AddWord("vim", new Style(foreground: Color.Yellow))
        .AddWord("init", new Style(foreground: Color.Blue))
        .AddWord("push", new Style(foreground: Color.Red))
        .AddWord("commit", new Style(foreground: Color.Blue))
        .AddWord("rebase", new Style(foreground: Color.Red))
        .AddWord("Hello", new Style(foreground: Color.Blue))
        .AddWord("SHIFT", new Style(foreground: Color.Grey))
        .AddWord("ENTER", new Style(foreground: Color.Grey))
        .AddWord("ConsoleLineEditor", new Style(foreground: Color.Yellow, decoration: Decoration.SlowBlink)),
};

// Add some history
editor.History.Add("foo\nbar\nbaz");
editor.History.Add("bar");
editor.History.Add("Spectre.Console");
editor.History.Add("What?");

// Add custom commands
editor.KeyBindings.Add<InsertSmiley>(ConsoleKey.I, ConsoleModifiers.Control);

// Read a line (or many)
var result = await editor.ReadLine(text, CancellationToken.None);

// Write the buffer
AnsiConsole.WriteLine();
AnsiConsole.Write(new Panel(result.EscapeMarkup())
    .Header("[yellow]Text:[/]")
    .RoundedBorder());

public sealed class InsertSmiley : LineEditorCommand
{
    public override void Execute(LineEditorContext context)
    {
        context.Buffer.Insert(":-)");
    }
}

public sealed class TestCompletion : ITextCompletion
{
    public IEnumerable<string>? GetCompletions(string context, string word, string suffix)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            return ["git", "code", "vim"];
        }

        if (context.Equals("git ", StringComparison.Ordinal))
        {
            return ["init", "branch", "push", "commit", "rebase"];
        }

        return null;
    }
}