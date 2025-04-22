using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoleLineEditor.Tests.Utilities;
using Shouldly;
using Spectre.Console.Testing;
using Xunit;

namespace ConsoleLineEditor.Tests;

public sealed class LineEditorTests
{
    [Fact]
    public async Task Should_Return_Entered_Text_When_Pressing_Enter()
    {
        // Given
        var editor = new LineEditor(
            new TestConsole(),
            new TestInputSource()
                .Push("Patrik")
                .PushEnter());

        // When
        var result = await editor.ReadLine(CancellationToken.None);

        // Then
        result.ShouldBe("Patrik");
    }

    [Fact]
    public async Task Should_Add_New_Line_When_Pressing_Shift_And_Enter()
    {
        // Given
        var editor = new LineEditor(
            new TestConsole(),
            new TestInputSource()
                .Push("Patrik")
                .PushNewLine()
                .Push("Svensson")
                .PushEnter())
        {
            MultiLine = true
        };

        // When
        var result = await editor.ReadLine(CancellationToken.None);

        // Then
        result.ShouldBe($"Patrik{Environment.NewLine}Svensson");
    }

    [Fact]
    public async Task Should_Move_To_Previous_Item_In_History()
    {
        // Given
        var editor = new LineEditor(
            new TestConsole(),
            new TestInputSource()
                .Push(ConsoleKey.PageUp)
                .Push(ConsoleKey.PageUp)
                .Push(ConsoleKey.PageUp)
                .PushEnter());

        editor.History.Add("Foo");
        editor.History.Add("Bar");
        editor.History.Add("Baz");

        // When
        var result = await editor.ReadLine(CancellationToken.None);

        // Then
        result.ShouldBe("Foo");
    }

    [Fact]
    public async Task Should_Move_To_Next_Item_In_History()
    {
        // Given
        var editor = new LineEditor(
            new TestConsole(),
            new TestInputSource()
                .Push(ConsoleKey.PageUp)
                .Push(ConsoleKey.PageUp)
                .Push(ConsoleKey.PageUp)
                .Push(ConsoleKey.PageDown)
                .Push(ConsoleKey.PageDown)
                .PushEnter());

        editor.History.Add("Foo");
        editor.History.Add("Bar");
        editor.History.Add("Baz");

        // When
        var result = await editor.ReadLine(CancellationToken.None);

        // Then
        result.ShouldBe("Baz");
    }
}