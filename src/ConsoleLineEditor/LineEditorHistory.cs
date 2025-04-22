using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLineEditor.Internal;
using ConsoleLineEditor.Internal.Extensions;

namespace ConsoleLineEditor;

internal sealed class LineEditorHistory : ILineEditorHistory
{
    private readonly LinkedList<LineBuffer[]> _history = [];
    private LinkedListNode<LineBuffer[]>? _current;
    private LineBuffer[]? _intermediate;
    private bool _showIntermediate;
    private int _maxItems = 50;

    public int Count => _history.Count;
    public LineBuffer[]? Current => _showIntermediate && _intermediate != null ? _intermediate : _current?.Value;

    public int MaxItems
    {
        get => _maxItems;
        set
        {
            _maxItems = Math.Max(0, value);

            while (_history.Count > _maxItems)
            {
                _history.RemoveFirst();
            }
        }
    }

    public void Add(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var lines = text.NormalizeNewLines().Split('\n');
        var buffers = new LineBuffer[lines.Length];
        for (var index = 0; index < lines.Length; index++)
        {
            buffers[index] = new LineBuffer(lines[index]);
        }

        Add(buffers);
    }

    internal void Reset()
    {
        _current = null;
        _intermediate = null;
    }

    private void Add(IList<LineBuffer> buffers)
    {
        ArgumentNullException.ThrowIfNull(buffers);

        var shouldAdd = _history.Last == null;
        if (_history.Last != null)
        {
            if (_history.Last.Value.Length != buffers.Count)
            {
                // Not the same length so not the same content.
                shouldAdd = true;
            }
            else
            {
                // Compare the buffers line by line
                for (var index = 0; index < buffers.Count; index++)
                {
                    // Not the same content?
                    if (!buffers[index].Content.Equals(_history.Last.Value[index].Content, StringComparison.Ordinal))
                    {
                        shouldAdd = true;
                        break;
                    }
                }
            }
        }

        if (shouldAdd)
        {
            _history.AddLast(buffers as LineBuffer[] ?? buffers.ToArray());

            if (_history.Count > _maxItems)
            {
                _history.RemoveFirst();
            }
        }

        _current = null;
    }

    internal bool MovePrevious(LineEditorState state)
    {
        // At the last one?
        if ((_current == null && _intermediate == null) || _showIntermediate)
        {
            // Got something written that we don't want to lose?
            // Store the intermediate buffer so it won't get lost.
            _intermediate = state.GetBuffers().ToArray();
        }

        _showIntermediate = false;

        if (_current == null && _history.Count > 0)
        {
            _current = _history.Last;
            return true;
        }

        if (_current?.Previous != null)
        {
            _current = _current.Previous;
            return true;
        }

        return false;
    }

    internal bool MoveNext()
    {
        if (_current == null)
        {
            return false;
        }

        if (_current?.Next != null)
        {
            _current = _current.Next;
            return true;
        }

        // Got an intermediate buffer to show?
        if (_intermediate != null)
        {
            _showIntermediate = true;
            _current = null;
            return true;
        }

        return false;
    }
}