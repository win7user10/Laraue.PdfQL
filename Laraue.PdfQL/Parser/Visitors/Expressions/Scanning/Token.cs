namespace Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

public class Token
{
    public required TokenType TokenType { get; init; }
    public required string? Lexeme { get; init; }
    public object? Literal { get; init; }
}