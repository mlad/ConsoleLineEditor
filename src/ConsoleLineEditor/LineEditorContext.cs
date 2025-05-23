using System;
using System.Collections.Generic;

namespace ConsoleLineEditor;

public sealed class LineEditorContext : IServiceProvider
{
    private readonly Dictionary<string, object?> _state;
    private readonly IServiceProvider? _provider;

    public LineBuffer Buffer { get; }
    internal SubmitAction? Result { get; private set; }

    public LineEditorContext(LineBuffer buffer, IServiceProvider? provider = null)
    {
        _state = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        _provider = provider;

        Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
    }

    public object? GetService(Type serviceType)
    {
        return _provider?.GetService(serviceType);
    }

    public void Execute(LineEditorCommand command)
    {
        if (Result != null)
        {
            // Don't execute any command
            // if we're supposed to exit the
            // current context.
            return;
        }

        command.Execute(this);
    }

    public void SetState<T>(string key, T value)
    {
        _state[key] = value;
    }

    public T GetState<T>(string key, Func<T> defaultValue)
    {
        if (_state.TryGetValue(key, out var value))
        {
            if (value is T typedValue)
            {
                return typedValue;
            }
        }

        return defaultValue();
    }

    public void Submit(SubmitAction action)
    {
        Result = action;
    }
}