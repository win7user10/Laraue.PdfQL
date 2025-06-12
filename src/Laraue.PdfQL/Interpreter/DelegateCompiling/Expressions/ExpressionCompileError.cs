using Laraue.PdfQL.Interpreter.Scanning;

namespace Laraue.PdfQL.Interpreter.DelegateCompiling.Expressions;

public class ExpressionCompileError
{
    public Token? Token { get; set; }
    public required string Error { get; set; }
}