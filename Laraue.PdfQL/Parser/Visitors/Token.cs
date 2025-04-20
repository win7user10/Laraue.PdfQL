namespace Laraue.PdfQL.Parser.Visitors;

public class Token
{
    public Token(string value)
    {
        Value = value;
    }

    public string Value { get; set; }
    public required string Name { get; set; }
}