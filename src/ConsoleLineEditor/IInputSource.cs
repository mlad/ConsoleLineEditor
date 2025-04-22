using System;

namespace ConsoleLineEditor;

public interface IInputSource
{
    bool ByPassProcessing { get; }

    bool IsKeyAvailable();
    ConsoleKeyInfo ReadKey();
}