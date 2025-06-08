using Laraue.PdfQL.Interpreter.Parsing.Stages;

namespace Laraue.PdfQL.Interpreter.Parsing;

public class ParseResult
{
    public required List<Stage> Stages { get; set; }
    public required ParseError[] Errors { get; set; }
    public bool HasErrors => Errors.Any();
}