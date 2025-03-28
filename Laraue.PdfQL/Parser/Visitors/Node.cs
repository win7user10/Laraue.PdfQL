namespace Laraue.PdfQL.Parser.Visitors;

public class Node
{
    public string Value { get; set; } = string.Empty;

    public override string ToString()
    {
        return Value;
    }
}