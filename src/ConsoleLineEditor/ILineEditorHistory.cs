namespace ConsoleLineEditor;

public interface ILineEditorHistory
{
    int Count { get; }
    void Add(string text);
    int MaxItems { get; set; }
}