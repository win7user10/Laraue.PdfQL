using Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing;

public class ParseResult
{
    public Expr? Expression { get; set; }
    public required ParseError[] Errors { get; set; }
    public bool HasErrors => Errors.Any();
}