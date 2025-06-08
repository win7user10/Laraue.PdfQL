using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.DelegateCompiling.Expressions;

public class ExpressionCompileError
{
    public Token? Token { get; set; }
    public required string Error { get; set; }
}