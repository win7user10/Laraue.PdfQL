namespace Laraue.PdfQL.Parser.Visitors;

public class TokenWithGroupNumber : Token
{
    public int GroupNumber { get; set; }
    public int Priority { get; set; }
}