namespace Laraue.PdfQL.Parser.Visitors;

public class TokenGroup
{
    public ICollection<Token> Tokens { get; set; } = new List<Token>();
}