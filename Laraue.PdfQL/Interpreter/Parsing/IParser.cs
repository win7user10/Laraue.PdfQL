using Laraue.PdfQL.Interpreter.Scanning;

namespace Laraue.PdfQL.Interpreter.Parsing;

public interface IParser
{
    ParseResult Parse(Token[] tokens);
}