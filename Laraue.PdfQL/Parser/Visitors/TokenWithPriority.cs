namespace Laraue.PdfQL.Parser.Visitors;

public class TokenWithPriority : TokenBase
{
    public TokenWithPriority(string value)
    {
        Value = value;
    }

    public override string Value { get; }
    public required string Name { get; set; }
    public int Priority { get; set; }
}