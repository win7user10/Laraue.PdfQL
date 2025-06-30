using Laraue.PdfQL.Interpreter.Scanning;

namespace Laraue.PdfQL.Interpreter.Parsing;

public class ParseError
{
    public required Token Token { get; init; }
    public required string Error { get; init; }
}